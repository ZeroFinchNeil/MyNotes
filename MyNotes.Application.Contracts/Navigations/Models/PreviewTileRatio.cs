using MyNotes.Common.Structures;

namespace MyNotes.Application.Contracts.Navigations.Models;

public enum PreviewTileRatio
{
  Shorter,    // 4:2 (0.50)
  Short,      // 4:3 (0.75)
  Square,     // 4:4 (1.00)
  Tall,       // 4:5 (1.25)
  Taller      // 4:6 (1.50)
}

public sealed class PreviewTileRatioSettingsCodec : ISettingsCodec<PreviewTileRatio, int>
{
  public static PreviewTileRatioSettingsCodec Default => field ??= new();

  private PreviewTileRatioSettingsCodec() { }
  public int Encode(PreviewTileRatio input) => (int)input;

  public PreviewTileRatio Decode(int output) => (PreviewTileRatio)output;
}