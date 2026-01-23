using System;

using Windows.Storage;

namespace MyNotes.Services.Settings;

/// <summary>
/// 로컬 애플리케이션 데이터 저장소에 애플리케이션 설정을 저장, 검색 및 모니터링하기 위한 메서드와 이벤트를 제공합니다.
/// </summary>
/// <remarks>
/// SettingsService는 키와 값을 사용하여 애플리케이션 설정을 읽고 쓸 수 있도록 합니다.
/// 이 서비스는 Windows.Storage.ApplicationDataContainer와 호환되는 다양한 데이터 형식을 지원합니다.
/// 또한 설정이 변경될 때 이벤트를 발생시켜 설정 값 업데이트에 대응할 수 있도록 합니다.
/// 지원되는 데이터 형식 및 사용 패턴에 대한 자세한 내용은 <see href="https://learn.microsoft.com/windows/apps/design/app-settings/store-and-retrieve-app-data">설정과 기타 앱 데이터의 저장 및 검색</see>을 참고하세요.
/// </remarks>
internal sealed class SettingsService
{
  public ApplicationDataContainer LocalSettings { get; } = ApplicationData.Current.LocalSettings;

  public void Save<T>(string settingsKey, T settingsValue) where T : notnull
  {
    LocalSettings.Values.TryGetValue(settingsKey, out var oldSettingsValue);
    LocalSettings.Values[settingsKey] = settingsValue;

    if (!settingsValue.Equals(oldSettingsValue))
      SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(settingsKey, typeof(T), oldSettingsValue, settingsValue));
  }

  /// <remarks>
  /// 앱 설정에 사용할 수 있는 데이터 유형(Type)은 다음과 같습니다.
  /// byte, short, ushort, int, uint, long, ulong, float, double,
  /// bool, char, string,
  /// System.DateTimeOffset, System.TimeSpan,
  /// System.Guid, Windows.Foundation.Point, Windows.Foundation.Size, Windows.Foundation.Rect,
  /// Windows.Storage.ApplicationDataCompositeValue
  /// </remarks>
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