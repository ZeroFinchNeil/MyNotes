namespace MyNotes.Common.Structures;

public sealed record SettingsDescriptor<T>(string Key, T DefaultValue) { }