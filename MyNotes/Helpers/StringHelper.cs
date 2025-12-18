namespace MyNotes.Helpers;

internal static class StringHelper
{
  public static string? NullIfWhiteSpace(string? str) => string.IsNullOrWhiteSpace(str) ? null : str;

  public static string SizeToString(int width, int height) => $"{width} × {height}";

  public static bool IsNullOrWhiteSpace(string? str) => string.IsNullOrWhiteSpace(str);
  public static bool IsNullOrEmpty(string? str) => string.IsNullOrEmpty(str);
  public static bool IsFilled(string? str) => !string.IsNullOrWhiteSpace(str);
  public static bool IsNotEmpty(string? str) => !string.IsNullOrEmpty(str);
}
