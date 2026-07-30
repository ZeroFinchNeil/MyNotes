using System;
using System.Collections.Generic;

namespace MyNotes.Application.Contracts.Settings;

internal interface ISettingsStorage
{
  public bool IsValid(object value);

  public bool IsValidType(Type type);

  public void Save<T>(string settingsKey, T settingsValue) where T : notnull;

  public T? Load<T>(string settingsKey) where T : notnull;

  public void SaveToComposite<T>(string settingsKey, KeyValuePair<string, T> pair) where T : notnull;

  public IReadOnlyDictionary<string, T> LoadFromComposite<T>(string settingsKey) where T : notnull;
}