using MyNotes.Application.Contracts.Navigations.Models;

namespace MyNotes.Common.Layout;

internal static class PreviewTileRatioMetrics
{
  public static double GetRatio(PreviewTileRatio ratio) => ratio switch
  {
    PreviewTileRatio.Shorter => 0.50,
    PreviewTileRatio.Short => 0.75,
    PreviewTileRatio.Square => 1.00,
    PreviewTileRatio.Tall => 1.25,
    PreviewTileRatio.Taller => 1.50,
    _ => throw new ArgumentOutOfRangeException(nameof(ratio))
  };

  public static PreviewTileRatio FromRatio(double ratio) => ratio switch
  {
    < 0.625 => PreviewTileRatio.Shorter,
    < 0.875 => PreviewTileRatio.Short,
    < 1.125 => PreviewTileRatio.Square,
    < 1.375 => PreviewTileRatio.Tall,
    _ => PreviewTileRatio.Taller
  };
}