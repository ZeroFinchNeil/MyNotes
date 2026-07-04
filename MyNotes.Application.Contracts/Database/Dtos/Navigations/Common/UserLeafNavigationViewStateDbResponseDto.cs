using System;

namespace MyNotes.Application.Contracts.Database.Dtos.Navigations.Common;

internal sealed record UserLeafNavigationViewStateDbResponseDto : UserNavigationViewStateDbResponseDto
{
  public required int? NoteSortKey { get; init; }

  public required int? NoteSortDirection { get; init; }

  public required int? PreviewLayoutType { get; init; }

  public required int? PreviewTileSize { get; init; }

  public required int? PreviewTileRatio { get; init; }
}