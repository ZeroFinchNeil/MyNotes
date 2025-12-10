namespace MyNotes.Helpers;

internal static class StringHelper
{
  public static string? NullIfWhiteSpace(this string? str) => string.IsNullOrWhiteSpace(str) ? null : str;

  public static string SizeToString(int width, int height) => $"{width} × {height}";
}
