using System.Text.RegularExpressions;

namespace MyNotes.Constants;

public static partial class AppRegexes
{
  [GeneratedRegex(@"(\\par[\s\r\n]*)+}", RegexOptions.Multiline)]
  public static partial Regex LastParInRtfRegex();
}
