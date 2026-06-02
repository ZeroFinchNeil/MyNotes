using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.WinUI.Helpers;

using Microsoft.Windows.Globalization;

using MyNotes.Common.Collections;
using MyNotes.Common.Messages;
using MyNotes.Constants;
using MyNotes.Domain.ValueObjects;
using MyNotes.Services.Navigations;
using MyNotes.Services.Settings;
using MyNotes.Shared.Constants;
using MyNotes.Shared.Enums.Navigations;
using MyNotes.Shared.Enums.Notes;
using MyNotes.Shared.Enums.Settings;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Navigations.Providers;

using Windows.ApplicationModel;
using Windows.System.UserProfile;

namespace MyNotes.ViewModels;

internal sealed partial class SettingsViewModel : ViewModelBase
{
  private readonly SettingsService SettingsService;
  private readonly NavigationController NavigationController;
  private readonly NavigationViewModelProvider NavigationViewModelProvider;

  #region Object Lifetime Management
  public SettingsViewModel(SettingsService settingsService, NavigationController navigationController, NavigationViewModelProvider navigationViewModelProvider)
  {
    SettingsService = settingsService;
    NavigationController = navigationController;
    NavigationViewModelProvider = navigationViewModelProvider;

    // Appearance
    AppTheme = SettingsService.Load(AppSettingsDescriptors.AppTheme);
    AppLanguage = new AppLanguage(SettingsService.Load(AppSettingsDescriptors.AppLanguage));

    // General
    InitialPageId = SettingsService.Load(AppSettingsDescriptors.InitialPageId);
    InitialPageType = SettingsService.Load(AppSettingsDescriptors.InitialPageType);

    ConfirmBeforeDeleting = SettingsService.Load(AppSettingsDescriptors.ConfirmBeforeDeleting);

    // Note
    NoteBackground = SettingsService.Load(AppSettingsDescriptors.NoteBackground).ToColor();
    NoteBackdrop = SettingsService.Load(AppSettingsDescriptors.NoteBackdropKind);

    var noteSize = SettingsService.Load(AppSettingsDescriptors.NoteSize);
    NoteWidth = (int)noteSize.Width;
    NoteHeight = (int)noteSize.Height;

    DeleteEmptyNote = SettingsService.Load(AppSettingsDescriptors.DeleteEmptyNote);

    // List and Group
    ShowNoteCount = SettingsService.Load(AppSettingsDescriptors.ShowNoteCount);
    _groupIconBadge = SettingsService.Load(AppSettingsDescriptors.GroupIconBadge);

    AllowCustomNoteSortOrder = SettingsService.Load(AppSettingsDescriptors.AllowCustomNoteSortOrder);
    NoteSortKey = SettingsService.Load(AppSettingsDescriptors.NoteSortKey);
    NoteSortDirection = SettingsService.Load(AppSettingsDescriptors.NoteSortDirection);

    AllowCustomPreviewLayout = SettingsService.Load(AppSettingsDescriptors.AllowCustomPreviewLayout);
    PreviewLayoutType = SettingsService.Load(AppSettingsDescriptors.PreviewLayoutType);
    PreviewTileSize = SettingsService.Load(AppSettingsDescriptors.PreviewTileSize);
    PreviewTileRatio = SettingsService.Load(AppSettingsDescriptors.PreviewTileRatio);

    SettingsService.SettingsChanged += SettingsService_SettingsChanged;

    RegisterMessengers();
  }

  protected override void Dispose(bool disposing)
  {
    if (Disposed)
    {
      return;
    }

    if (disposing)
    {
      SettingsService.SettingsChanged -= SettingsService_SettingsChanged;
      UnregisterMessengers();
    }

    base.Dispose(disposing);
  }
  #endregion

  private static readonly Dictionary<string, Action<SettingsViewModel, SettingsChangedEventArgs>> SettingsMap = new()
  {
    { AppSettingsDescriptors.AppTheme.Key, (v, e) => v.AppTheme = (int)e.NewSettingsValue },
    { AppSettingsDescriptors.AppLanguage.Key, (v, e) => v.AppLanguage = new AppLanguage((string)e.NewSettingsValue) },
    { AppSettingsDescriptors.InitialPageType.Key, (v, e) => v.InitialPageType = (int)e.NewSettingsValue },
    { AppSettingsDescriptors.InitialPageId.Key, (v, e) => v.InitialPageId = (Guid)e.NewSettingsValue },
    { AppSettingsDescriptors.NoteBackground.Key, (v, e) => v.NoteBackground = ((string)e.NewSettingsValue).ToColor() },
    { AppSettingsDescriptors.NoteBackdropKind.Key, (v, e) => v.NoteBackdrop = (int)e.NewSettingsValue },
    { AppSettingsDescriptors.NoteSize.Key, (v, e) =>
      {
        if(e.NewSettingsValue is SizeInt32 size)
        {
          v.NoteWidth = size.Width;
          v.NoteHeight = size.Height;
        }
      }},
    { AppSettingsDescriptors.ShowNoteCount.Key, (v, e) => v.ShowNoteCount = (bool)e.NewSettingsValue },
    { AppSettingsDescriptors.GroupIconBadge.Key, (v, e) => v.GroupIconBadge = (int)e.NewSettingsValue },
    { AppSettingsDescriptors.AllowCustomNoteSortOrder.Key, (v, e) => v.AllowCustomNoteSortOrder = (bool)e.NewSettingsValue },
    { AppSettingsDescriptors.NoteSortKey.Key, (v, e) => v.NoteSortKey = (int)e.NewSettingsValue },
    { AppSettingsDescriptors.NoteSortDirection.Key, (v, e) => v.NoteSortDirection = (int)e.NewSettingsValue },
    { AppSettingsDescriptors.AllowCustomPreviewLayout.Key, (v, e) => v.AllowCustomPreviewLayout = (bool)e.NewSettingsValue },
    { AppSettingsDescriptors.PreviewLayoutType.Key, (v, e) => v.PreviewLayoutType = (int)e.NewSettingsValue },
    { AppSettingsDescriptors.PreviewTileSize.Key, (v, e) => v.PreviewTileSize = (int)e.NewSettingsValue },
    { AppSettingsDescriptors.PreviewTileRatio.Key, (v, e) => v.PreviewTileRatio = (int)e.NewSettingsValue },
  };

  private void SettingsService_SettingsChanged(object? sender, SettingsChangedEventArgs e)
  {
    if (SettingsMap.TryGetValue(e.SettingsKey, out var action))
    {
      action.Invoke(this, e);
    }
  }

  #region Appearance
  public int AppTheme
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        ValueChangedMessage<ElementTheme> msg = value switch
        {
          1 => new(ElementTheme.Light),
          2 => new(ElementTheme.Dark),
          _ => new(ElementTheme.Default)
        };
        WeakReferenceMessenger.Default.Send(msg, AppMessageTokens.ChangeAppThemeToken);
        SettingsService.Save(AppSettingsDescriptors.AppTheme, value);
      }
    }
  }

  public List<AppLanguage> AppLanguages { get; } = new(AppLanguage.ManifestLanguages.Keys.Select(lang => new AppLanguage(lang)));
  private static readonly AppLanguage _initialLanguage = new(ApplicationLanguages.Languages[0]);

  public AppLanguage AppLanguage
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        string language = value.Language;

        SettingsService.Save(AppSettingsDescriptors.AppLanguage, language);

        try
        {
          ApplicationLanguages.PrimaryLanguageOverride = string.IsNullOrEmpty(language) ? GlobalizationPreferences.Languages[0] : language;
        }
        catch
        {
          ApplicationLanguages.PrimaryLanguageOverride = GlobalizationPreferences.Languages[0];
        }

        IsAppLanguageChanged = (value != _initialLanguage);
      }
    }
  }

  [ObservableProperty]
  public partial bool IsAppLanguageChanged { get; set; }
  #endregion

  #region General
  public Guid InitialPageId
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        Console.WriteLine("{0}: {1}", "InitialPageId", value);
        SettingsService.Save(AppSettingsDescriptors.InitialPageId, value);
      }
    }
  }

  public int InitialPageType
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        switch (value)
        {
          case (int)Models.Navigations.InitialPageType.Home:
            InitialPageId = NavigationId.Home.Value;
            break;
          case (int)Models.Navigations.InitialPageType.Bookmarks:
            InitialPageId = NavigationId.Bookmarks.Value;
            break;
          case (int)Models.Navigations.InitialPageType.LastOpened:
            SetLastOpenedInitialPage();
            break;
          case (int)Models.Navigations.InitialPageType.Preferred:
            SetPreferredInitialPageOptions();
            break;
        }
        SettingsService.Save(AppSettingsDescriptors.InitialPageType, value);
      }
    }
  }

  public ObservableCollection<UserListNavigationViewModel> InitialPageOptions { get; } = new();
  public UserListNavigationViewModel? SelectedInitialPageOption
  {
    get;
    set
    {
      if (SetProperty(ref field, value) && value is not null)
      {
        InitialPageId = value.Navigation.Id.Value;
      }
    }
  }

  private void SetLastOpenedInitialPage()
  {
    if (InitialPageId == NavigationId.Home.Value || InitialPageId == NavigationId.Bookmarks.Value)
    {
      return;
    }

    var viewmodels = NavigationViewModelProvider.Resolve<UserListNavigationViewModel>(NavigationController.UserLeafNavigations);
    if (viewmodels.FirstOrDefault(vm => vm.Navigation.Id.Value == InitialPageId) is null)
    {
      InitialPageId = viewmodels.Count > 0 ? viewmodels[0].Navigation.Id.Value : NavigationId.Home.Value;
    }

    //RequestMessage<IReadOnlyList<UserListNavigationViewModel>> message = new();
    //WeakReferenceMessenger.Default.Send(message, AppMessageTokens.GetAllListNavigationViewModelsToken);
    //if (message.HasReceivedResponse)
    //{
    //  var viewmodels = message.Response;
    //  if (viewmodels.FirstOrDefault(vm => vm.Navigation.Id.Value == InitialPageId) is null)
    //  {
    //    InitialPageId = viewmodels.Count > 0
    //      ? viewmodels[0].Navigation.Id.Value
    //      : NavigationId.Home.Value;
    //  }
    //}
  }

  private void SetPreferredInitialPageOptions()
  {
    var previousSelection = SelectedInitialPageOption;

    foreach (var viewmodel in NavigationViewModelProvider.Resolve<UserListNavigationViewModel>(NavigationController.UserLeafNavigations))
    {
      InitialPageOptions.Add(viewmodel);
    }

    //RequestMessage<IReadOnlyList<UserListNavigationViewModel>> message = new();
    //WeakReferenceMessenger.Default.Send(message, AppMessageTokens.GetAllListNavigationViewModelsToken);
    //if (message.HasReceivedResponse)
    //{
    //  InitialPageOptions.Clear();
    //  foreach (var viewmodel in message.Response)
    //  {
    //    InitialPageOptions.Add(viewmodel);
    //  }
    //}

    if (previousSelection is not null && InitialPageOptions.Contains(previousSelection))
    {
      SelectedInitialPageOption = previousSelection;
    }
    else if (InitialPageOptions.FirstOrDefault(vm => vm.Navigation.Id.Value == InitialPageId) is UserListNavigationViewModel lastOpened)
    {
      SelectedInitialPageOption = lastOpened;
    }
    else
    {
      SelectedInitialPageOption = InitialPageOptions.FirstOrDefault();
    }
  }

  public bool ConfirmBeforeDeleting
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        SettingsService.Save(AppSettingsDescriptors.ConfirmBeforeDeleting, value);
      }
    }
  }
  #endregion

  #region Note
  public Color NoteBackground
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        SettingsService.Save(AppSettingsDescriptors.NoteBackground, value.ToString());
      }
    }
  }

  public int NoteBackdrop
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        SettingsService.Save(AppSettingsDescriptors.NoteBackdropKind, value);
      }
    }
  }

  public int NoteWidth
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        SettingsService.Save(AppSettingsDescriptors.NoteSize, new Size(value, NoteHeight));
      }
    }
  }

  public int NoteHeight
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        SettingsService.Save(AppSettingsDescriptors.NoteSize, new Size(NoteWidth, value));
      }
    }
  }

  public bool DeleteEmptyNote
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        SettingsService.Save(AppSettingsDescriptors.DeleteEmptyNote, value);
      }
    }
  }
  #endregion

  #region List
  public bool ShowNoteCount
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        SettingsService.Save(AppSettingsDescriptors.ShowNoteCount, value);
      }
    }
  }

  private int _groupIconBadge;
  public int GroupIconBadge
  {
    get => _groupIconBadge;
    set
    {
      if (SetProperty(ref _groupIconBadge, value))
      {
        SettingsService.Save(AppSettingsDescriptors.GroupIconBadge, value);
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<GroupIconBadge>((GroupIconBadge)value), AppMessageTokens.ChangeNavigationViewModelIconImageToken);
      }
    }
  }

  [ObservableProperty]
  public partial string SortOrderContentText { get; set; } = string.Empty;

  private static readonly IReadOnlyDictionary<NoteSortKey, Func<string>> _noteSortKeyLocalizedStringMap = new Dictionary<NoteSortKey, Func<string>>()
  {
    { Shared.Enums.Notes.NoteSortKey.Created, () => LocalizedStrings.NoteSortKeyCreated },
    { Shared.Enums.Notes.NoteSortKey.Modified, () => LocalizedStrings.NoteSortKeyModified },
  };

  private static readonly IReadOnlyDictionary<SortDirection, Func<string>> _noteSortDirectionLocalizedStringMap = new Dictionary<SortDirection, Func<string>>()
  {
    { SortDirection.Ascending, () => LocalizedStrings.SortDirectionAscending },
    { SortDirection.Descending, () => LocalizedStrings.SortDirectionDescending },
  };

  private static string SetSortOrderContentText(NoteSortKey noteSortKey, SortDirection sortDirection) => $"{_noteSortKeyLocalizedStringMap[noteSortKey].Invoke()} • {_noteSortDirectionLocalizedStringMap[sortDirection].Invoke()}";
  private static string SetSortOrderContentText(int noteSortKey, int sortDirection) => SetSortOrderContentText((NoteSortKey)noteSortKey, (SortDirection)sortDirection);

  public bool AllowCustomNoteSortOrder
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        SettingsService.Save(AppSettingsDescriptors.AllowCustomNoteSortOrder, value);
      }
    }
  }

  public int NoteSortKey
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        SettingsService.Save(AppSettingsDescriptors.NoteSortKey, value);
        SortOrderContentText = SetSortOrderContentText(value, NoteSortDirection);
      }
    }
  }

  public int NoteSortDirection
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        SettingsService.Save(AppSettingsDescriptors.NoteSortDirection, value);
        SortOrderContentText = SetSortOrderContentText(NoteSortKey, value);
      }
    }
  }

  public bool AllowCustomPreviewLayout
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        SettingsService.Save(AppSettingsDescriptors.AllowCustomPreviewLayout, value);
      }
    }
  }

  private static readonly IReadOnlyDictionary<PreviewLayoutType, Func<string>> _previewLayoutTypeLocalizedStringMap = new Dictionary<PreviewLayoutType, Func<string>>()
  {
    { Shared.Enums.Navigations.PreviewLayoutType.Grid, () => LocalizedStrings.PreviewLayoutTypeGrid },
    { Shared.Enums.Navigations.PreviewLayoutType.List, () => LocalizedStrings.PreviewLayoutTypeList },
  };

  public IReadOnlyDictionary<PreviewLayoutType, Func<string>> PreviewLayoutTypeLocalizedStringMap => _previewLayoutTypeLocalizedStringMap;

  private static readonly IReadOnlyDictionary<PreviewTileSize, Func<string>> _previewTileSizeLocalizedStringMap = new Dictionary<PreviewTileSize, Func<string>>()
  {
    { Shared.Enums.Navigations.PreviewTileSize.Smallest, () => LocalizedStrings.PreviewTileSizeSmallest },
    { Shared.Enums.Navigations.PreviewTileSize.Smaller, () => LocalizedStrings.PreviewTileSizeSmaller  },
    { Shared.Enums.Navigations.PreviewTileSize.Small, () => LocalizedStrings.PreviewTileSizeSmall },
    { Shared.Enums.Navigations.PreviewTileSize.Medium, () => LocalizedStrings.PreviewTileSizeMedium },
    { Shared.Enums.Navigations.PreviewTileSize.Large, () => LocalizedStrings.PreviewTileSizeLarge },
    { Shared.Enums.Navigations.PreviewTileSize.Larger, () => LocalizedStrings.PreviewTileSizeLarger },
    { Shared.Enums.Navigations.PreviewTileSize.Largest, () => LocalizedStrings.PreviewTileSizeLargest},
  };

  private static readonly IReadOnlyDictionary<PreviewTileRatio, Func<string>> _previewTileRatioLocalizedStringMap = new Dictionary<PreviewTileRatio, Func<string>>()
  {
    { Shared.Enums.Navigations.PreviewTileRatio.Shorter, () => LocalizedStrings.PreviewTileRatioShorter },
    { Shared.Enums.Navigations.PreviewTileRatio.Short, () => LocalizedStrings.PreviewTileRatioShort },
    { Shared.Enums.Navigations.PreviewTileRatio.Square, () => LocalizedStrings.PreviewTileRatioSquare },
    { Shared.Enums.Navigations.PreviewTileRatio.Tall, () => LocalizedStrings.PreviewTileRatioTall },
    { Shared.Enums.Navigations.PreviewTileRatio.Taller, () => LocalizedStrings.PreviewTileRatioTaller },
  };

  private static string SetPreviewLayoutContentText(PreviewLayoutType previewLayoutType, PreviewTileSize previewTileSize, PreviewTileRatio previewTileRatio) => $"{_previewLayoutTypeLocalizedStringMap[previewLayoutType].Invoke()} • {_previewTileSizeLocalizedStringMap[previewTileSize].Invoke()} • {_previewTileRatioLocalizedStringMap[previewTileRatio].Invoke()}";
  private static string SetPreviewLayoutContentText(int previewLayoutType, int previewTileSize, int previewTileRatio) =>
    SetPreviewLayoutContentText((PreviewLayoutType)previewLayoutType, (PreviewTileSize)previewTileSize, (PreviewTileRatio)previewTileRatio);

  [ObservableProperty]
  public partial string PreviewLayoutContentText { get; set; } = string.Empty;

  public int PreviewLayoutType
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        SettingsService.Save(AppSettingsDescriptors.PreviewLayoutType, value);
        PreviewLayoutContentText = SetPreviewLayoutContentText(value, PreviewTileSize, PreviewTileRatio);
      }
    }
  }

  public int PreviewTileSize
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        SettingsService.Save(AppSettingsDescriptors.PreviewTileSize, value);
        PreviewLayoutContentText = SetPreviewLayoutContentText(PreviewLayoutType, value, PreviewTileRatio);
      }
    }
  }

  public int PreviewTileRatio
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        SettingsService.Save(AppSettingsDescriptors.PreviewTileRatio, value);
        PreviewLayoutContentText = SetPreviewLayoutContentText(PreviewLayoutType, PreviewTileSize, value);
      }
    }
  }
  #endregion

#pragma warning disable CA1822
  // StartupTask
  public async Task<bool> GetStartupTaskState()
  {
    StartupTask startupTask = await StartupTask.GetAsync(AppStrings.StartupTaskId);
    return startupTask.State is StartupTaskState.Enabled or StartupTaskState.EnabledByPolicy;
  }

  public async Task<bool> ToggleStartupTaskState()
  {
    StartupTask startupTask = await StartupTask.GetAsync(AppStrings.StartupTaskId);
    switch (startupTask.State)
    {
      case StartupTaskState.Enabled:
        startupTask.Disable();
        return false;
      case StartupTaskState.EnabledByPolicy:
        return true;
      default:
        await startupTask.RequestEnableAsync();
        return await GetStartupTaskState();
    }
  }
#pragma warning restore CA1822

  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<ValueChangedMessage<bool>, MessageToken>(this, AppMessageTokens.NavigationCollectionChangedToken, (recipient, message) =>
    {
      if (InitialPageType == (int)Models.Navigations.InitialPageType.Preferred)
      {
        SetPreferredInitialPageOptions();
      }
    });
  }

  private void UnregisterMessengers() => WeakReferenceMessenger.Default.UnregisterAll(this);
}
