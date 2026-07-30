namespace MyNotes.Application.Contracts.Navigations.Models;

public enum PreviewLayoutType
{
  Grid,
  List
}

internal static class PreviewLayoutTypeSettingsCodec
{
  public static int Encode(PreviewLayoutType input) => (int)input;

  public static PreviewLayoutType Decode(int output) => (PreviewLayoutType)output;
}