using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.WinUI.Helpers;

using Microsoft.Windows.Globalization;

using MyNotes.Constants;
using MyNotes.Models.Navigations;
using MyNotes.Models.Settings;
using MyNotes.Services.Settings;

using Windows.ApplicationModel;
using Windows.System.UserProfile;

namespace MyNotes.ViewModels;

internal sealed partial class SettingsViewModel : ViewModelBase
{
  private readonly SettingsService SettingsService;

  public SettingsViewModel(SettingsService settingsService)
  {
    SettingsService = settingsService;

    AppTheme = SettingsService.Load(SettingsDescriptors.AppTheme);
    AppLanguage = new AppLanguage(SettingsService.Load(SettingsDescriptors.AppLanguage));
    InitialPageType = SettingsService.Load(SettingsDescriptors.InitialPageType);
    InitialPageId = SettingsService.Load(SettingsDescriptors.InitialPageId);

    NoteBackground = SettingsService.Load(SettingsDescriptors.NoteBackground).ToColor();
    NoteBackdrop = SettingsService.Load(SettingsDescriptors.NoteBackdrop);

    var noteSize = SettingsService.Load(SettingsDescriptors.NoteSize);
    NoteWidth = (int)noteSize.Width;
    NoteHeight = (int)noteSize.Height;

    ShowNoteCount = SettingsService.Load(SettingsDescriptors.ShowNoteCount);
    _groupIconBadge = SettingsService.Load(SettingsDescriptors.GroupIconBadge);

    AllowCustomNoteSortOrder = SettingsService.Load(SettingsDescriptors.AllowCustomNoteSortOrder);
    NoteSortKey = SettingsService.Load(SettingsDescriptors.NoteSortKey);
    NoteSortDirection = SettingsService.Load(SettingsDescriptors.NoteSortDirection);

    AllowCustomPreviewLayout = SettingsService.Load(SettingsDescriptors.AllowCustomPreviewLayout);
    PreviewLayoutType = SettingsService.Load(SettingsDescriptors.PreviewLayoutType);
    PreviewTileSize = SettingsService.Load(SettingsDescriptors.PreviewTileSize);
    PreviewTileRatio = SettingsService.Load(SettingsDescriptors.PreviewTileRatio);

    SettingsService.SettingsChanged += SettingsService_SettingsChanged;
  }

  protected override void Dispose(bool disposing)
  {
    if (_disposed)
      return;

    if (disposing)
    {
      SettingsService.SettingsChanged -= SettingsService_SettingsChanged;
    }

    _disposed = true;
  }

  private static readonly Dictionary<string, Action<SettingsViewModel, SettingsChangedEventArgs>> SettingsMap = new()
  {
    { SettingsDescriptors.AppTheme.Key, (v, e) => v.AppTheme = (int)e.NewSettingsValue },
    { SettingsDescriptors.AppLanguage.Key, (v, e) => v.AppLanguage = new AppLanguage((string)e.NewSettingsValue) },
    { SettingsDescriptors.InitialPageType.Key, (v, e) => v.InitialPageType = (int)e.NewSettingsValue },
    { SettingsDescriptors.InitialPageId.Key, (v, e) => v.InitialPageId = (Guid)e.NewSettingsValue },
    { SettingsDescriptors.NoteBackground.Key, (v, e) => v.NoteBackground = ((string)e.NewSettingsValue).ToColor() },
    { SettingsDescriptors.NoteBackdrop.Key, (v, e) => v.NoteBackdrop = (int)e.NewSettingsValue },
    { SettingsDescriptors.NoteSize.Key, (v, e) =>
      {
        if(e.NewSettingsValue is SizeInt32 size)
        {
          v.NoteWidth = size.Width;
          v.NoteHeight = size.Height;
        }
      }},
    { SettingsDescriptors.ShowNoteCount.Key, (v, e) => v.ShowNoteCount = (bool)e.NewSettingsValue },
    { SettingsDescriptors.GroupIconBadge.Key, (v, e) => v.GroupIconBadge = (int)e.NewSettingsValue },
    { SettingsDescriptors.AllowCustomNoteSortOrder.Key, (v, e) => v.AllowCustomNoteSortOrder = (bool)e.NewSettingsValue },
    { SettingsDescriptors.NoteSortKey.Key, (v, e) => v.NoteSortKey = (int)e.NewSettingsValue },
    { SettingsDescriptors.NoteSortDirection.Key, (v, e) => v.NoteSortDirection = (int)e.NewSettingsValue },
    { SettingsDescriptors.AllowCustomPreviewLayout.Key, (v, e) => v.AllowCustomPreviewLayout = (bool)e.NewSettingsValue },
    { SettingsDescriptors.PreviewLayoutType.Key, (v, e) => v.PreviewLayoutType = (int)e.NewSettingsValue },
    { SettingsDescriptors.PreviewTileSize.Key, (v, e) => v.PreviewTileSize = (int)e.NewSettingsValue },
    { SettingsDescriptors.PreviewTileRatio.Key, (v, e) => v.PreviewTileRatio = (int)e.NewSettingsValue },
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
      if (field != value)
      {
        SetProperty(ref field, value);
        ValueChangedMessage<ElementTheme> msg = value switch
        {
          1 => new(ElementTheme.Light),
          2 => new(ElementTheme.Dark),
          _ => new(ElementTheme.Default)
        };
        WeakReferenceMessenger.Default.Send(msg, MessageTokens.AppThmeChangedToken);
        SettingsService.Save(SettingsDescriptors.AppTheme, value);
      }
    }
  }

  public List<AppLanguage> AppLanguages { get; } = new(AppLanguage.ManifestLanguages.Keys.Select(lang => new AppLanguage(lang)));
  private static readonly AppLanguage _initalLanguage = new(ApplicationLanguages.Languages[0]);

  public AppLanguage AppLanguage
  {
    get;
    set
    {
      if (field != value)
      {
        SetProperty(ref field, value);
        string language = value.Language;

        SettingsService.Save(SettingsDescriptors.AppLanguage, language);

        try
        {
          ApplicationLanguages.PrimaryLanguageOverride = string.IsNullOrEmpty(language) ? GlobalizationPreferences.Languages[0] : language;
        }
        catch
        {
          ApplicationLanguages.PrimaryLanguageOverride = GlobalizationPreferences.Languages[0];
        }

        IsAppLanguageChanged = (value != _initalLanguage);
      }
    }
  }

  public bool IsAppLanguageChanged
  {
    get;
    set => SetProperty(ref field, value);
  }
  #endregion

  #region General
  public int InitialPageType
  {
    get;
    set
    {
      if (field != value)
      {
        SetProperty(ref field, value);
        InitialPageId = value switch
        {
          0 => NavigationId.Home.Value,
          1 => NavigationId.Bookmarks.Value,
          2 => NavigationId.Empty.Value,
          3 => NavigationId.Empty.Value,
          _ => NavigationId.Home.Value
        };
        SettingsService.Save(SettingsDescriptors.InitialPageType, value);
      }
    }
  }

  public Guid InitialPageId
  {
    get;
    set
    {
      if (field != value)
      {
        SetProperty(ref field, value);
        SettingsService.Save(SettingsDescriptors.InitialPageId, value);
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
      if (field != value)
      {
        SetProperty(ref field, value);
        SettingsService.Save(SettingsDescriptors.NoteBackground, value.ToString());
      }
    }
  }

  public int NoteBackdrop
  {
    get;
    set
    {
      if (field != value)
      {
        SetProperty(ref field, value);
        SettingsService.Save(SettingsDescriptors.NoteBackdrop, value);
      }
    }
  }

  public int NoteWidth
  {
    get;
    set
    {
      if (field != value)
      {
        SetProperty(ref field, value);
        SettingsService.Save(SettingsDescriptors.NoteSize, new Size(value, NoteHeight));
      }
    }
  }

  public int NoteHeight
  {
    get;
    set
    {
      if (field != value)
      {
        SetProperty(ref field, value);
        SettingsService.Save(SettingsDescriptors.NoteSize, new Size(NoteWidth, value));
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
      if (field != value)
      {
        SetProperty(ref field, value);
        SettingsService.Save(SettingsDescriptors.ShowNoteCount, value);
      }
    }
  }

  private int _groupIconBadge;
  public int GroupIconBadge
  {
    get => _groupIconBadge;
    set
    {
      if (_groupIconBadge != value)
      {
        SetProperty(ref _groupIconBadge, value);
        SettingsService.Save(SettingsDescriptors.GroupIconBadge, value);
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<GroupIconBadge>((GroupIconBadge)value), MessageTokens.ChangeNavigationViewModelIconImageToken);
      }
    }
  }

  public bool AllowCustomNoteSortOrder
  {
    get;
    set
    {
      if (field != value)
      {
        SetProperty(ref field, value);
        SettingsService.Save(SettingsDescriptors.AllowCustomNoteSortOrder, value);
      }
    }
  }

  public int NoteSortKey
  {
    get;
    set
    {
      if (field != value)
      {
        SetProperty(ref field, value);
        SettingsService.Save(SettingsDescriptors.NoteSortKey, value);
      }
    }
  }

  public int NoteSortDirection
  {
    get;
    set
    {
      if (field != value)
      {
        SetProperty(ref field, value);
        SettingsService.Save(SettingsDescriptors.NoteSortDirection, value);
      }
    }
  }

  public bool AllowCustomPreviewLayout
  {
    get;
    set
    {
      if (field != value)
      {
        SetProperty(ref field, value);
        SettingsService.Save(SettingsDescriptors.AllowCustomPreviewLayout, value);
      }
    }
  }

  public int PreviewLayoutType
  {
    get;
    set
    {
      if (field != value)
      {
        SetProperty(ref field, value);
        SettingsService.Save(SettingsDescriptors.PreviewLayoutType, value);
      }
    }
  }

  public int PreviewTileSize
  {
    get;
    set
    {
      if (field != value)
      {
        SetProperty(ref field, value);
        SettingsService.Save(SettingsDescriptors.PreviewTileSize, value);
      }
    }
  }

  public int PreviewTileRatio
  {
    get;
    set
    {
      if (field != value)
      {
        SetProperty(ref field, value);
        SettingsService.Save(SettingsDescriptors.PreviewTileRatio, value);
      }
    }
  }
  #endregion

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
}
