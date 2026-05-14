using System;

namespace MyNotes.Application.Contracts.Database.Enums.Navigations;

[Flags]
internal enum UserNavigationViewStateUpdateFields
{
  None,
  IsExpanded,
  RestorePrevious,
  RestoreNext,
  NoteSortKey,
  NoteSortDirection,
  PreviewLayoutType,
  PreviewTileSize,
  PreviewTileRatio
}