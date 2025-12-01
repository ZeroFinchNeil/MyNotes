using Windows.Storage;

namespace MyNotes.Services.Settings;

internal sealed class SettingsService
{
  // 앱 설정(LocalSettings) 사용 방법은 다음 페이지를 참고하세요.
  // https://learn.microsoft.com/windows/apps/design/app-settings/store-and-retrieve-app-data

  public ApplicationDataContainer LocalSettings { get; } = ApplicationData.Current.LocalSettings;

  // 앱 설정에 사용할 수 있는 데이터 유형은 다음과 같습니다.
  // byte, short, ushort, int, uint, long, ulong, float, double
  // bool, char, string
  // System.DateTimeOffset, System.TimeSpan
  // System.Guid, Windows.Foundation.Point, Windows.Foundation.Size, Windows.Foundation.Rect
  // Windows.Storage.ApplicationDataCompositeValue

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
