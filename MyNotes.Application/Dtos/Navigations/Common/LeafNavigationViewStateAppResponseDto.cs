using MyNotes.Common.Querying;
using MyNotes.Domain.ValueObjects;
using MyNotes.Shared.Enums.Navigations;
using MyNotes.Shared.Enums.Notes;

namespace MyNotes.Application.Dtos.Navigations.Common;

internal sealed record LeafNavigationViewStateAppResponseDto : NavigationViewStateAppResponseDto
{
  public required NoteSortKey? NoteSortKey { get; init; }

  public required SortDirection? NoteSortDirection { get; init; }

  public required PreviewLayoutType? PreviewLayoutType { get; init; }

  public required PreviewTileSize? PreviewTileSize { get; init; }

  public required PreviewTileRatio? PreviewTileRatio { get; init; }
}