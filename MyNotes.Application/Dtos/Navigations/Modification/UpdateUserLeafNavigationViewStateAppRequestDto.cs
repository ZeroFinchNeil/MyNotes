using MyNotes.Application.Contracts.Database.Enums.Navigations;
using MyNotes.Common.Querying;
using MyNotes.Shared.Enums.Navigations;
using MyNotes.Shared.Enums.Notes;

namespace MyNotes.Application.Dtos.Navigations.Modification;

internal sealed record UpdateUserLeafNavigationViewStateAppRequestDto : UpdateUserNavigationViewStateAppRequestDto
{
  public required UserLeafNavigationViewStateUpdateFields UpdateFields { get; init; }

  public NoteSortKey? NoteSortKey { get; init; }

  public SortDirection? NoteSortDirection { get; init; }

  public PreviewLayoutType? PreviewLayoutType { get; init; }

  public PreviewTileSize? PreviewTileSize { get; init; }

  public PreviewTileRatio? PreviewTileRatio { get; init; }
}