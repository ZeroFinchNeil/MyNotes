using MyNotes.Common.Structures;

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

public sealed class PreviewTileSizeSettingsCodec : ISettingsCodec<PreviewTileSize, int>
{
  public static PreviewTileSizeSettingsCodec Default => field ??= new();

  private PreviewTileSizeSettingsCodec() { }
  public int Encode(PreviewTileSize input) => (int)input;

  public PreviewTileSize Decode(int output) => (PreviewTileSize)output;
}