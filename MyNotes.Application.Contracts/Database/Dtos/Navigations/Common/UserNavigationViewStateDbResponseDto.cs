using System;
using System.Collections.Generic;
using System.Text;

namespace MyNotes.Application.Contracts.Database.Dtos.Navigations.Common;

internal sealed record UserNavigationViewStateDbResponseDto
{
  public required Guid Id { get; init; }

  public required bool IsExpanded { get; init; }

  public required Guid? RestorePrevious { get; init; }

  public required Guid? RestoreNext { get; init; }

  public required int? NoteSortKey { get; init; }

  public required int? NoteSortDirection { get; init; }

  public required int? PreviewLayoutType { get; init; }

  public required int? PreviewTileSize { get; init; }

  public required int? PreviewTileRatio { get; init; }
}
