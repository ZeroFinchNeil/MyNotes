using System;
using System.Diagnostics.CodeAnalysis;

namespace MyNotes.Common.Structures;

public readonly record struct SettingsDescriptor<T>
{
  public required string Key { get; init; }

  public required T DefaultValue { get; init; }

  [SetsRequiredMembers]
  public SettingsDescriptor(string key, T defaultValue)
  {
    Key = key;
    DefaultValue = defaultValue;
  }

  public SettingsDescriptor<TResult> Convert<TResult>(Converter<T, TResult> converter) => new()
  {
    Key = this.Key,
    DefaultValue = converter(this.DefaultValue)
  };
}