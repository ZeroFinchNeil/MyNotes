using MyNotes.Application.Contracts.Settings;
using MyNotes.Common.Structures;

namespace MyNotes.Services.Settings;

internal sealed class ViewStateSettingsService
{
  private readonly ISettingsStorage SettingsStorage;

  public ViewStateSettingsService(ISettingsStorage settingsStorage)
  {
    SettingsStorage = settingsStorage;
  }

  public void Save<T>(SettingsDescriptor<T> settings, T settingsValue) where T : notnull
  {
    if (!SettingsStorage.IsValidType(typeof(T)))
    {
      throw new InvalidOperationException();
    }

    var oldSettingsValue = SettingsStorage.Load<T>(settings.Key);
    if (oldSettingsValue is null || !oldSettingsValue.Equals(settingsValue))
    {
      SettingsStorage.Save(settings.Key, settingsValue);
    }
  }

  public void Save<T, TResult>(Converter<T, TResult> converter, SettingsDescriptor<T> settings, T settingsValue) where T : notnull where TResult : notnull => Save(settings.Convert(converter), converter(settingsValue));

  public T Load<T>(SettingsDescriptor<T> settings) where T : notnull => SettingsStorage.IsValidType(typeof(T))
    ? SettingsStorage.Load<T>(settings.Key) is T TValue ? TValue : settings.DefaultValue
    : throw new InvalidOperationException();

  public T Load<T, TSource>(Converter<TSource, T> converter, SettingsDescriptor<T> settings) where T : notnull where TSource : notnull => SettingsStorage.IsValidType(typeof(TSource))
    ? SettingsStorage.Load<TSource>(settings.Key) is TSource TValue ? converter(TValue) : settings.DefaultValue
    : throw new InvalidOperationException();
}