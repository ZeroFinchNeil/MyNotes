namespace MyNotes.Services.Settings;

public sealed record SettingsDescriptor<T>(string Key, T DefaultValue) { }