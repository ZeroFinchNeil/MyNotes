using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.WinUI.Helpers;

using Microsoft.Windows.Globalization;

using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Querying.Models;
using MyNotes.Application.Navigations;
using MyNotes.Application.Notes;
using MyNotes.Application.Settings.Services;
using MyNotes.Common.Messages;
using MyNotes.Constants;
using MyNotes.Debugging;
using MyNotes.Domain.Navigations;
using MyNotes.Models.Navigations.Preferences;
using MyNotes.Models.UI;
using MyNotes.Services.Navigations;
using MyNotes.Strings;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Navigations.Providers;

using Windows.ApplicationModel;
using Windows.System.UserProfile;

namespace MyNotes.ViewModels;

internal sealed partial class SettingsViewModel : ViewModelBase
{
  private readonly AppSettingsService AppSettingsService;
  private readonly NavigationController NavigationController;
  private readonly NavigationViewModelProvider NavigationViewModelProvider;

  #region Object Lifetime Management
  public SettingsViewModel(AppSettingsService appSettingsService, NavigationController navigationController, NavigationViewModelProvider navigationViewModelProvider)
  {
    AppSettingsService = appSettingsService;
    NavigationController = navigationController;
    NavigationViewModelProvider = navigationViewModelProvider;

    // Appearance
    AppTheme = AppSettingsService.Load<ElementTheme, int>(ElementThemeSettingsCodec.Decode, AppSettingsDescriptors.AppTheme);
    AppLanguage = AppSettingsService.Load<AppLanguage, string>(AppLanguageSettingsCodec.Decode, AppSettingsDescriptors.AppLanguage);

    // General
    InitialPageId = AppSettingsService.Load(AppSettingsDescriptors.InitialPageId);
    InitialPageType = AppSettingsService.Load<InitialPageType, int>(InitialPageTypeSettingsCodec.Decode, AppSettingsDescriptors.InitialPageType);

    ConfirmBeforeDeleting = AppSettingsService.Load(AppSettingsDescriptors.ConfirmBeforeDeleting);

    // Note
    NoteBackground = AppSettingsService.Load(NoteSettingsDescriptors.NoteBackground).ToColor();
    NoteBackdrop = AppSettingsService.Load<BackdropKind, int>(BackdropKindSettingsCodec.Decode, NoteSettingsDescriptors.NoteBackdropKind);

    var noteSize = AppSettingsService.Load<SizeInt32, Size>(s => new((int)s.Width, (int)s.Height), AppSettingsDescriptors.DefaultNoteSize);
    NoteWidth = noteSize.Width;
    NoteHeight = noteSize.Height;

    DeleteEmptyNote = AppSettingsService.Load(AppSettingsDescriptors.DeleteEmptyNote);

    // List and Group
    ShowNoteCount = AppSettingsService.Load(AppSettingsDescriptors.ShowNoteCount);
    _groupIconBadge = AppSettingsService.Load<GroupIconBadge, int>(GroupIconBadgeSettingsCodec.Decode, AppSettingsDescriptors.GroupIconBadge);

    AllowCustomNoteSortOrder = AppSettingsService.Load(AppSettingsDescriptors.AllowCustomNoteSortOrder);
    NoteSortKey = AppSettingsService.Load<NoteSortKey, int>(NoteSortKeySettingsCodec.Decode, NavigationSettingsDescriptors.NoteSortKey);
    NoteSortDirection = AppSettingsService.Load<SortDirection, int>(SortDirectionSettingsCodec.Decode, NavigationSettingsDescriptors.NoteSortDirection);

    AllowCustomPreviewLayout = AppSettingsService.Load(AppSettingsDescriptors.AllowCustomPreviewLayout);
    PreviewLayoutType = AppSettingsService.Load<PreviewLayoutType, int>(PreviewLayoutTypeSettingsCodec.Decode, NavigationSettingsDescriptors.PreviewLayoutType);
    PreviewTileSize = AppSettingsService.Load<PreviewTileSize, int>(PreviewTileSizeSettingsCodec.Decode, NavigationSettingsDescriptors.PreviewTileSize);
    PreviewTileRatio = AppSettingsService.Load<PreviewTileRatio, int>(PreviewTileRatioSettingsCodec.Decode, NavigationSettingsDescriptors.PreviewTileRatio);

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
      UnregisterMessengers();
    }

    base.Dispose(disposing);
  }
  #endregion

  #region Appearance
  public ElementTheme AppTheme
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        ValueChangedMessage<ElementTheme> msg = new(value);
        WeakReferenceMessenger.Default.Send(msg, AppMessageTokens.ChangeAppThemeToken);
        AppSettingsService.Save(ElementThemeSettingsCodec.Encode, AppSettingsDescriptors.AppTheme, value);
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

        AppSettingsService.Save(AppLanguageSettingsCodec.Encode, AppSettingsDescriptors.AppLanguage, value);

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
        ConsoleHelper.WriteLine(true, "{0}: {1}", "InitialPageId", value);
        AppSettingsService.Save(AppSettingsDescriptors.InitialPageId, value);
      }
    }
  }

  public InitialPageType InitialPageType
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        switch (value)
        {
          case InitialPageType.Home:
            InitialPageId = NavigationId.Home.Value;
            break;
          case InitialPageType.Bookmarks:
            InitialPageId = NavigationId.Bookmarks.Value;
            break;
          case InitialPageType.LastOpened:
            SetLastOpenedInitialPage();
            break;
          case InitialPageType.Preferred:
            SetPreferredInitialPageOptions();
            break;
        }
        AppSettingsService.Save(InitialPageTypeSettingsCodec.Encode, AppSettingsDescriptors.InitialPageType, value);
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

    //foreach(var navigation in NavigationController.UserLeafNavigations)
    //{

    //}
    //var viewmodels = NavigationController.UserLeafNavigations.Select(n => NavigationViewModelProvider.Resolve(n).ViewModel).OfType(;
    //if (viewmodels.FirstOrDefault(vm => vm.Navigation.Id.Value == InitialPageId) is null)
    //{
    //  InitialPageId = viewmodels.Count > 0 ? viewmodels[0].Navigation.Id.Value : NavigationId.Home.Value;
    //}

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

    //foreach (var viewmodel in NavigationViewModelProvider.Resolve(NavigationController.UserLeafNavigations).Select(lease => lease.ViewModel).OfType<UserListNavigationViewModel>())
    //{
    //  InitialPageOptions.Add(viewmodel);
    //}

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
        AppSettingsService.Save(AppSettingsDescriptors.ConfirmBeforeDeleting, value);
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
        AppSettingsService.Save(NoteSettingsDescriptors.NoteBackground, value.ToString());
      }
    }
  }

  public BackdropKind NoteBackdrop
  {
    get;
    set
    {
      if (Enum.IsDefined(value) && SetProperty(ref field, value))
      {
        AppSettingsService.Save(BackdropKindSettingsCodec.Encode, NoteSettingsDescriptors.NoteBackdropKind, value);
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
        AppSettingsService.Save(SizeInt32SettingsCodec.Encode, AppSettingsDescriptors.DefaultNoteSize, new SizeInt32(value, NoteHeight));
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
        AppSettingsService.Save(SizeInt32SettingsCodec.Encode, AppSettingsDescriptors.DefaultNoteSize, new SizeInt32(NoteWidth, value));
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
        AppSettingsService.Save(AppSettingsDescriptors.DeleteEmptyNote, value);
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
        AppSettingsService.Save(AppSettingsDescriptors.ShowNoteCount, value);
      }
    }
  }

  private GroupIconBadge _groupIconBadge;
  public GroupIconBadge GroupIconBadge
  {
    get => _groupIconBadge;
    set
    {
      if (SetProperty(ref _groupIconBadge, value))
      {
        AppSettingsService.Save(GroupIconBadgeSettingsCodec.Encode, AppSettingsDescriptors.GroupIconBadge, value);
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<GroupIconBadge>(value), AppMessageTokens.ChangeNavigationViewModelIconImageToken);
      }
    }
  }

  [ObservableProperty]
  public partial string SortOrderContentText { get; set; } = string.Empty;

  private static readonly IReadOnlyDictionary<NoteSortKey, Func<string>> _noteSortKeyLocalizedStringMap = new Dictionary<NoteSortKey, Func<string>>()
  {
    { NoteSortKey.Created, () => LocalizedStrings.NoteSortKeyCreated },
    { NoteSortKey.Modified, () => LocalizedStrings.NoteSortKeyModified },
  };

  private static readonly IReadOnlyDictionary<SortDirection, Func<string>> _noteSortDirectionLocalizedStringMap = new Dictionary<SortDirection, Func<string>>()
  {
    { SortDirection.Ascending, () => LocalizedStrings.SortDirectionAscending },
    { SortDirection.Descending, () => LocalizedStrings.SortDirectionDescending },
  };

  private static string SetSortOrderContentText(NoteSortKey noteSortKey, SortDirection sortDirection) => $"{_noteSortKeyLocalizedStringMap[noteSortKey].Invoke()} • {_noteSortDirectionLocalizedStringMap[sortDirection].Invoke()}";

  public bool AllowCustomNoteSortOrder
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        AppSettingsService.Save(AppSettingsDescriptors.AllowCustomNoteSortOrder, value);
      }
    }
  }

  public NoteSortKey NoteSortKey
  {
    get;
    set
    {
      if (Enum.IsDefined(value) && SetProperty(ref field, value))
      {
        AppSettingsService.Save(NoteSortKeySettingsCodec.Encode, NavigationSettingsDescriptors.NoteSortKey, value);
        SortOrderContentText = SetSortOrderContentText(value, NoteSortDirection);
      }
    }
  }

  public SortDirection NoteSortDirection
  {
    get;
    set
    {
      if (Enum.IsDefined(value) && SetProperty(ref field, value))
      {
        AppSettingsService.Save(SortDirectionSettingsCodec.Encode, NavigationSettingsDescriptors.NoteSortDirection, value);
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
        AppSettingsService.Save(AppSettingsDescriptors.AllowCustomPreviewLayout, value);
      }
    }
  }

  private static readonly IReadOnlyDictionary<PreviewLayoutType, Func<string>> _previewLayoutTypeLocalizedStringMap = new Dictionary<PreviewLayoutType, Func<string>>()
  {
    { PreviewLayoutType.Grid, () => LocalizedStrings.PreviewLayoutTypeGrid },
    { PreviewLayoutType.List, () => LocalizedStrings.PreviewLayoutTypeList },
  };

  public IReadOnlyDictionary<PreviewLayoutType, Func<string>> PreviewLayoutTypeLocalizedStringMap => _previewLayoutTypeLocalizedStringMap;

  private static readonly IReadOnlyDictionary<PreviewTileSize, Func<string>> _previewTileSizeLocalizedStringMap = new Dictionary<PreviewTileSize, Func<string>>()
  {
    { PreviewTileSize.Smallest, () => LocalizedStrings.PreviewTileSizeSmallest },
    { PreviewTileSize.Smaller, () => LocalizedStrings.PreviewTileSizeSmaller  },
    { PreviewTileSize.Small, () => LocalizedStrings.PreviewTileSizeSmall },
    { PreviewTileSize.Medium, () => LocalizedStrings.PreviewTileSizeMedium },
    { PreviewTileSize.Large, () => LocalizedStrings.PreviewTileSizeLarge },
    { PreviewTileSize.Larger, () => LocalizedStrings.PreviewTileSizeLarger },
    { PreviewTileSize.Largest, () => LocalizedStrings.PreviewTileSizeLargest},
  };

  private static readonly IReadOnlyDictionary<PreviewTileRatio, Func<string>> _previewTileRatioLocalizedStringMap = new Dictionary<PreviewTileRatio, Func<string>>()
  {
    { PreviewTileRatio.Shorter, () => LocalizedStrings.PreviewTileRatioShorter },
    { PreviewTileRatio.Short, () => LocalizedStrings.PreviewTileRatioShort },
    { PreviewTileRatio.Square, () => LocalizedStrings.PreviewTileRatioSquare },
    { PreviewTileRatio.Tall, () => LocalizedStrings.PreviewTileRatioTall },
    { PreviewTileRatio.Taller, () => LocalizedStrings.PreviewTileRatioTaller },
  };

  private static string SetPreviewLayoutContentText(PreviewLayoutType previewLayoutType, PreviewTileSize previewTileSize, PreviewTileRatio previewTileRatio) => $"{_previewLayoutTypeLocalizedStringMap[previewLayoutType].Invoke()} • {_previewTileSizeLocalizedStringMap[previewTileSize].Invoke()} • {_previewTileRatioLocalizedStringMap[previewTileRatio].Invoke()}";

  [ObservableProperty]
  public partial string PreviewLayoutContentText { get; set; } = string.Empty;

  public PreviewLayoutType PreviewLayoutType
  {
    get;
    set
    {
      if (Enum.IsDefined(value) && SetProperty(ref field, value))
      {
        AppSettingsService.Save(PreviewLayoutTypeSettingsCodec.Encode, NavigationSettingsDescriptors.PreviewLayoutType, value);
        PreviewLayoutContentText = SetPreviewLayoutContentText(value, PreviewTileSize, PreviewTileRatio);
      }
    }
  }

  public PreviewTileSize PreviewTileSize
  {
    get;
    set
    {
      if (Enum.IsDefined(value) && SetProperty(ref field, value))
      {
        AppSettingsService.Save(PreviewTileSizeSettingsCodec.Encode, NavigationSettingsDescriptors.PreviewTileSize, value);
        PreviewLayoutContentText = SetPreviewLayoutContentText(PreviewLayoutType, value, PreviewTileRatio);
      }
    }
  }

  public PreviewTileRatio PreviewTileRatio
  {
    get;
    set
    {
      if (Enum.IsDefined(value) && SetProperty(ref field, value))
      {
        AppSettingsService.Save(PreviewTileRatioSettingsCodec.Encode, NavigationSettingsDescriptors.PreviewTileRatio, value);
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
      if (InitialPageType == InitialPageType.Preferred)
      {
        SetPreferredInitialPageOptions();
      }
    });
  }

  private void UnregisterMessengers() => WeakReferenceMessenger.Default.UnregisterAll(this);
}

internal static class ElementThemeSettingsCodec
{
  public static int Encode(ElementTheme input) => (int)input;

  public static ElementTheme Decode(int output) => (ElementTheme)output;
}

internal static class SizeInt32SettingsCodec
{
  public static Size Encode(SizeInt32 input) => new(input.Width, input.Height);

  public static SizeInt32 Decode(Size output) => new((int)output.Width, (int)output.Height);
}