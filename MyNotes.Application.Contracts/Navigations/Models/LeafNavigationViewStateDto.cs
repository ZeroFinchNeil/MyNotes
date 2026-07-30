using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Querying.Models;

namespace MyNotes.Application.Contracts.Navigations.Models;

internal sealed record LeafNavigationViewStateDto : NavigationViewStateDto
{
  public required NoteSortKey NoteSortKey { get; init; }

  public required SortDirection NoteSortDirection { get; init; }

  public required PreviewLayoutType PreviewLayoutType { get; init; }

  public required PreviewTileSize PreviewTileSize { get; init; }

  public required PreviewTileRatio PreviewTileRatio { get; init; }
}