using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Windows.Globalization;

using MyNotes.Models;
using MyNotes.Services.Settings;

using Windows.System.UserProfile;

namespace MyNotes.ViewModels;

public class SettingsViewModel : ViewModelBase
{
  private readonly SettingsService SettingsService;

  public SettingsViewModel(SettingsService settingsService)
  {
    SettingsService = settingsService;
    _initalLanguage = new(SettingsService.Load<string>(SettingsDescriptors.AppLanguage.Key));

    AppTheme = SettingsService.Load<int>(SettingsDescriptors.AppTheme.Key);
    AppLanguage = _initalLanguage;
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
}
