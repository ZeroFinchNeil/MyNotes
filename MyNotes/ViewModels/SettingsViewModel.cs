using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Windows.Globalization;

using MyNotes.Models;
using MyNotes.Services.Settings;
using MyNotes.Strings;

using Windows.ApplicationModel;
using Windows.System.UserProfile;

using ToolkitColorHelper = CommunityToolkit.WinUI.Helpers.ColorHelper;

namespace MyNotes.ViewModels;

public class SettingsViewModel : ViewModelBase
{
  private readonly SettingsService SettingsService;

  public SettingsViewModel(SettingsService settingsService)
  {
    SettingsService = settingsService;
    _initalLanguage = new(SettingsService.Load<string>(SettingsDescriptors.AppLanguage.Key) ?? SettingsDescriptors.AppLanguage.DefaultValue);

    AppTheme = SettingsService.Load<int?>(SettingsDescriptors.AppTheme.Key) ?? SettingsDescriptors.AppTheme.DefaultValue;
    AppLanguage = _initalLanguage;

    NoteBackground = ToolkitColorHelper.ToColor(SettingsService.Load<string>(SettingsDescriptors.NoteBackground.Key) ?? SettingsDescriptors.NoteBackground.DefaultValue);

    NoteBackdrop = SettingsService.Load<int?>(SettingsDescriptors.NoteBackdrop.Key) ?? SettingsDescriptors.NoteBackdrop.DefaultValue;

    var noteSize = SettingsService.Load<Size?>(SettingsDescriptors.NoteSize.Key) ?? SettingsDescriptors.NoteSize.DefaultValue;
    NoteWidth = (int)noteSize.Width;
    NoteHeight = (int)noteSize.Height;

    ShowNoteCount = SettingsService.Load<bool?>(SettingsDescriptors.ShowNoteCount.Key) ?? SettingsDescriptors.ShowNoteCount.DefaultValue;
  }

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
        WeakReferenceMessenger.Default.Send(msg, MessageTokens.ChangeAppTheme);
        SettingsService.Save(SettingsDescriptors.AppTheme.Key, value);
      }
    }
  }

  public List<AppLanguage> AppLanguages { get; } = new(AppLanguage.ManifestLanguages.Keys.Select(lang => new AppLanguage(lang)));

  private readonly AppLanguage _initalLanguage;
  public AppLanguage AppLanguage
  {
    get;
    set
    {
      if (field != value)
      {
        SetProperty(ref field, value);
        string language = value.Language;

        SettingsService.Save(SettingsDescriptors.AppLanguage.Key, language);

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

  public Color NoteBackground
  {
    get;
    set
    {
      if (field != value)
      {
        SetProperty(ref field, value);
        SettingsService.Save(SettingsDescriptors.NoteBackground.Key, value.ToString());
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
        SettingsService.Save(SettingsDescriptors.NoteBackdrop.Key, value);
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
        SettingsService.Save(SettingsDescriptors.NoteSize.Key, new Size(value, NoteHeight));
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
        SettingsService.Save(SettingsDescriptors.NoteSize.Key, new Size(NoteWidth, value));
      }
    }
  }

  public bool ShowNoteCount
  {
    get;
    set
    {
      if (field != value)
      {
        SetProperty(ref field, value);
        SettingsService.Save(SettingsDescriptors.ShowNoteCount.Key, value);
      }
    }
  }

  // StartupTask
  public async Task<bool> GetStartupTaskState()
  {
    StartupTask startupTask = await StartupTask.GetAsync(Resources.StartupTaskId);
    return startupTask.State is StartupTaskState.Enabled or StartupTaskState.EnabledByPolicy;
  }

  public async Task<bool> ToggleStartupTaskState()
  {
    StartupTask startupTask = await StartupTask.GetAsync(Resources.StartupTaskId);
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
