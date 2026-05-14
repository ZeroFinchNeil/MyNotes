using MyNotes.Common.Collections;
using MyNotes.Shared.Enums.Navigations;
using MyNotes.Shared.Enums.Notes;

namespace MyNotes.Application.Dtos.Navigations;

internal sealed record UserLeafNavigationAppResponseDto : UserNavigationAppResponseDto
{
  public required NoteSortKey? NoteSortKey { get; init; }

  public required SortDirection? NoteSortDirection { get; init; }

  public required PreviewLayoutType? PreviewLayoutType { get; init; }

  public required PreviewTileSize? PreviewTileSize { get; init; }

  public required PreviewTileRatio? PreviewTileRatio { get; init; }
}

/*
UserLeafNavigationAppResponseDto dto = new()
{
  Id = ,
  Parent = ,
  Icon = ,
  Title = ,
  Position = ,
  IsDeleted = ,
  NoteSortKey = ,
  NoteSortDirection = ,
  PreviewLayoutType = ,
  PreviewTileSize = ,
  PreviewTileRatio = ,
};
*/