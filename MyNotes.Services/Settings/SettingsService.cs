using System;

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

  public void Save<T>(string settingsKey, T settingsValue) where T : notnull
  {
    LocalSettings.Values.TryGetValue(settingsKey, out var oldSettingsValue);
    LocalSettings.Values[settingsKey] = settingsValue;

    if (!settingsValue.Equals(oldSettingsValue))
      SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(settingsKey, typeof(T), oldSettingsValue, settingsValue));
  }

  public void Save<T>(SettingsDescriptor<T> settings, T settingsValue) where T : notnull
  {
    LocalSettings.Values.TryGetValue(settings.Key, out var oldSettingsValue);
    LocalSettings.Values[settings.Key] = settingsValue;

    if (!settingsValue.Equals(oldSettingsValue))
      SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(settings.Key, typeof(T), oldSettingsValue, settingsValue));
  }

  public T? Load<T>(string settingsKey)
  {
    LocalSettings.Values.TryGetValue(settingsKey, out var value);
    return value is T TValue ? TValue : default;
  }

  public T Load<T>(SettingsDescriptor<T> settings)
  {
    LocalSettings.Values.TryGetValue(settings.Key, out var value);
    return value is T TValue ? TValue : settings.DefaultValue;
  }

  public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;
}

internal class SettingsChangedEventArgs : EventArgs
{
  public string SettingsKey { get; }
  public Type SettingsType { get; }
  public object? OldSettingsValue { get; }
  public object NewSettingsValue { get; }

  public SettingsChangedEventArgs(string settingsKey, Type settingsType, object? oldSettingsValue, object newSettingsValue)
  {
    SettingsKey = settingsKey;
    SettingsType = settingsType;

    if (oldSettingsValue is not null && oldSettingsValue.GetType() != settingsType && !settingsType.IsAssignableFrom(oldSettingsValue.GetType()))
    {
      throw new ArgumentException($"oldSettingsValue의 타입이 settingsType과 일치하지 않습니다. (Key: {settingsKey})");
    }
    if (newSettingsValue.GetType() != settingsType && !settingsType.IsAssignableFrom(newSettingsValue.GetType()))
    {
      throw new ArgumentException($"newSettingsValue의 타입이 settingsType과 일치하지 않습니다. (Key: {settingsKey})");
    }

    OldSettingsValue = oldSettingsValue;
    NewSettingsValue = newSettingsValue;
  }
}