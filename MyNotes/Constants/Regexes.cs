using System.Text.RegularExpressions;

namespace MyNotes.Constants;

internal static partial class Regexes
{
  [GeneratedRegex(@"(\\par[\s\r\n]*)+}", RegexOptions.Multiline)]
  public static partial Regex LastParInRtfRegex();
}
