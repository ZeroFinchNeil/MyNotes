using System.Text.Json;

namespace MyNotes.Shared.Constants;

public static class AppJson
{
  public static JsonSerializerOptions JsonSerializerOptions { get; } = new() { WriteIndented = true };
}
