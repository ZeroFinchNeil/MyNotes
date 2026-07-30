namespace MyNotes.Application.Contracts.Navigations.Models;

public enum PreviewTileSize
{
  Smallest,     // 120
  Smaller,      // 160
  Small,        // 200
  Medium,       // 240
  Large,        // 280
  Larger,       // 320
  Largest       // 360
}

public static class PreviewTileSizeSettingsCodec
{
  public static int Encode(PreviewTileSize input) => (int)input;

  public static PreviewTileSize Decode(int output) => (PreviewTileSize)output;
}