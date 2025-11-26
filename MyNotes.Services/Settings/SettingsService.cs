using Windows.Storage;

namespace MyNotes.Services.Settings;

public class SettingsService
{
  public ApplicationDataContainer LocalSettings { get; } = ApplicationData.Current.LocalSettings;

  public void Save<T>(string settingsKey, T settingsValue)
  {
    LocalSettings.Values[settingsKey] = settingsValue;
  }

  public T? Load<T>(string settingsKey)
  {
    LocalSettings.Values.TryGetValue(settingsKey, out var value);
    return value is T TValue ? TValue : default;
  }
}
