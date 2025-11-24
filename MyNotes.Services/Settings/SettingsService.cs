using Windows.Storage;

namespace MyNotes.Services.Settings;

public class SettingsService
{
  public static ApplicationDataContainer LocalSettings { get; } = ApplicationData.Current.LocalSettings;

  public static void Save<T>(string settingsKey, T settingsValue)
  {
    LocalSettings.Values[settingsKey] = settingsValue;
  }

  public static T? Load<T>(string settingsKey)
  {
    LocalSettings.Values.TryGetValue(settingsKey, out var value);
    return value is T TValue ? TValue : default;
  }
}
