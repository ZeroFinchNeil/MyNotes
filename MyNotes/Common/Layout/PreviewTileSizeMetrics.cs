using MyNotes.Application.Contracts.Navigations.Models;

namespace MyNotes.Common.Layout;

internal static class PreviewTileSizeMetrics
{
  public static double GetWidth(PreviewTileSize size) => size switch
  {
    PreviewTileSize.Smallest => 120.0,
    PreviewTileSize.Smaller => 160.0,
    PreviewTileSize.Small => 200.0,
    PreviewTileSize.Medium => 240.0,
    PreviewTileSize.Large => 280.0,
    PreviewTileSize.Larger => 320.0,
    PreviewTileSize.Largest => 360.0,
    _ => throw new ArgumentOutOfRangeException(nameof(size))
  };

  public static PreviewTileSize GetWidth(double width) => width switch
  {
    < 140.0 => PreviewTileSize.Smallest,
    < 180.0 => PreviewTileSize.Smaller,
    < 220.0 => PreviewTileSize.Small,
    < 260.0 => PreviewTileSize.Medium,
    < 300.0 => PreviewTileSize.Large,
    < 340.0 => PreviewTileSize.Larger,
    _ => PreviewTileSize.Largest
  };
}