using System.Text.Json;

namespace MyNotes.AppConstants;

internal static class AppJson
{
  public static JsonSerializerOptions JsonSerializerOptions { get; } = new() { WriteIndented = true };
}
