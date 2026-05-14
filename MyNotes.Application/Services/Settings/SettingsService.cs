using System;
using System.Collections.Generic;

using MyNotes.Common.Structures;

using Windows.Foundation.Collections;
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
  public static ApplicationDataContainer LocalSettings { get; } = ApplicationData.Current.LocalSettings;
  private readonly IPropertySet _settingsSet = LocalSettings.Values;

  public void Save<T>(string settingsKey, T settingsValue) where T : notnull
  {
    _settingsSet.TryGetValue(settingsKey, out var oldSettingsValue);
    _settingsSet[settingsKey] = settingsValue;

    if (!settingsValue.Equals(oldSettingsValue))
    {
      SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(settingsKey, typeof(T), oldSettingsValue, settingsValue));
    }
  }

  public static bool IsValid(object value)
  {
    switch (value)
    {
      case byte or short or ushort or int or uint or long or ulong or float or double or
        bool or char or string or System.DateTimeOffset or System.TimeSpan or
        System.Guid or Windows.Foundation.Point or Windows.Foundation.Size or Windows.Foundation.Rect or
        Windows.Storage.ApplicationDataCompositeValue:
        break;
      default:
        return false;
    }
    return true;
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
    if (!IsValid(settingsValue))
    {
      return;
    }

    _settingsSet.TryGetValue(settings.Key, out var oldSettingsValue);
    _settingsSet[settings.Key] = settingsValue;

    if (!settingsValue.Equals(oldSettingsValue))
    {
      SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(settings.Key, typeof(T), oldSettingsValue, settingsValue));
    }
  }

  public T? Load<T>(string settingsKey) where T : notnull
  {
    _settingsSet.TryGetValue(settingsKey, out var value);
    return value is T TValue ? TValue : default;
  }

  public T Load<T>(SettingsDescriptor<T> settings) where T : notnull
  {
    _settingsSet.TryGetValue(settings.Key, out var value);
    return value is T TValue ? TValue : settings.DefaultValue;
  }

  public void SaveToComposite<T>(SettingsDescriptor<T> settings, KeyValuePair<string, T> pair) where T : notnull
  {
    if (!IsValid(pair.Value))
    {
      return;
    }

    if (_settingsSet.TryGetValue(settings.Key, out var composite)
      && composite is ApplicationDataCompositeValue settingsPairs)
    {
      var oldSettingsValue = settingsPairs[pair.Key];
      settingsPairs[pair.Key] = pair.Value;
      if (!pair.Value.Equals(oldSettingsValue))
      {
        SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(settings.Key, typeof(T), oldSettingsValue, pair.Value));
      }
    }
    else
    {
      ApplicationDataCompositeValue compositeValue = new();
      compositeValue[pair.Key] = pair.Value;
      _settingsSet[settings.Key] = compositeValue;
      SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(settings.Key, typeof(T), null, pair.Value));
    }
  }

  public IReadOnlyDictionary<string, T> LoadFromComposite<T>(SettingsDescriptor<T> settings) where T : notnull
  {
    Dictionary<string, T> pairs = new();
    if (_settingsSet.TryGetValue(settings.Key, out var composite)
      && composite is ApplicationDataCompositeValue settingsPairs)
    {
      foreach (var kv in settingsPairs)
      {
        if (kv.Value is T TValue)
        {
          pairs.Add(kv.Key, TValue);
        }
      }
    }
    return pairs.AsReadOnly();
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