using MyNotes.Application.Contracts.Settings;
using MyNotes.Common.Structures;

namespace MyNotes.Application.Settings.Services;

internal sealed class AppSettingsService
{
  private readonly ISettingsStorage SettingsStorage;

  public AppSettingsService(ISettingsStorage settingsStorage)
  {
    SettingsStorage = settingsStorage;
  }

  public void Save<TSupported>(SettingsDescriptor<TSupported> settings, TSupported settingsValue) where TSupported : notnull
  {
    if (!SettingsStorage.IsValidType(typeof(TSupported)))
    {
      throw new InvalidOperationException();
    }

    if (!SettingsStorage.TryLoad<TSupported>(settings.Key, out var oldSettingsValue) || !oldSettingsValue.Equals(settingsValue))
    {
      SettingsStorage.Save(settings.Key, settingsValue);
    }
  }

  public void Save<T, TSupported>(ISettingsCodec<T, TSupported> codec, SettingsDescriptor<T> settings, T settingsValue) where T : notnull where TSupported : notnull => Save(settings.Convert(codec.Encode), codec.Encode(settingsValue));

  public TSupported Load<TSupported>(SettingsDescriptor<TSupported> settings) where TSupported : notnull => SettingsStorage.IsValidType(typeof(TSupported))
    ? SettingsStorage.TryLoad<TSupported>(settings.Key, out var settingsValue)
      ? settingsValue : settings.DefaultValue
    : throw new InvalidOperationException();

  public T Load<T, TSupported>(ISettingsCodec<T, TSupported> codec, SettingsDescriptor<T> settings) where T : notnull where TSupported : notnull => SettingsStorage.IsValidType(typeof(TSupported))
    ? SettingsStorage.TryLoad<TSupported>(settings.Key, out var supportedValue)
      ? codec.Decode(supportedValue)
      : settings.DefaultValue
    : throw new InvalidOperationException();
}