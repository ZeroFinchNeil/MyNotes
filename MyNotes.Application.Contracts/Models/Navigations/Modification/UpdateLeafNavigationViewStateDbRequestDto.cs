using MyNotes.Application.Contracts.Enums.Navigations;
using MyNotes.Common.Querying;
using MyNotes.Shared.Enums.Navigations;
using MyNotes.Shared.Enums.Notes;

namespace MyNotes.Application.Contracts.Models.Navigations.Modification;

internal sealed record UpdateLeafNavigationViewStateDbRequestDto : UpdateNavigationViewStateDbRequestDto
{
  public required LeafNavigationViewStateUpdateFields UpdateFields { get; init; }

  public required NoteSortKey? NoteSortKey { get; init; }

  public required SortDirection? NoteSortDirection { get; init; }

  public required PreviewLayoutType? PreviewLayoutType { get; init; }

  public required PreviewTileSize? PreviewTileSize { get; init; }

  public required PreviewTileRatio? PreviewTileRatio { get; init; }
}