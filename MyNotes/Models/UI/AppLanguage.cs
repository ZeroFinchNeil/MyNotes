using System.Globalization;

using Microsoft.Windows.Globalization;

using MyNotes.Common.Structures;
using MyNotes.Strings;

namespace MyNotes.Models.UI;

internal readonly record struct AppLanguage
{
  public static SortedList<string, CultureInfo> ManifestLanguages { get; } = new();

  static AppLanguage()
  {
    ManifestLanguages.Add(string.Empty, new CultureInfo(string.Empty));
    foreach (string lang in ApplicationLanguages.ManifestLanguages)
    {
      ManifestLanguages.Add(lang, new CultureInfo(lang));
    }
  }

  public string Language { get; }
  public string DisplayName { get; }
  public string NativeName { get; }

  public AppLanguage(string? language)
  {
    if (!string.IsNullOrEmpty(language) && ManifestLanguages.TryGetValue(language, out var cultureInfo))
    {
      Language = language;
      DisplayName = cultureInfo.DisplayName;
      NativeName = cultureInfo.NativeName;
    }
    else
    {
      Language = string.Empty;
      DisplayName = string.Empty;
      NativeName = LocalizedStrings.AppLanguageUseSystem;
    }
  }

  public AppLanguage() : this(string.Empty) { }

  public string FormatLanguageString(string lang1, string lang2) => lang1 == lang2 ? string.Empty : lang1;
}

internal sealed class AppLanguageSettingsCodec : ISettingsCodec<AppLanguage, string>
{
  public static AppLanguageSettingsCodec Default => field ??= new();

  private AppLanguageSettingsCodec() { }

  public string Encode(AppLanguage input) => input.Language;

  public AppLanguage Decode(string output) => new(output);
}
