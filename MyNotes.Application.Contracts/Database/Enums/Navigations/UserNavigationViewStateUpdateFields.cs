using System;

namespace MyNotes.Application.Contracts.Database.Enums.Navigations;

[Flags]
internal enum UserNavigationViewStateUpdateFields
{
  None = 0,
  IsExpanded = 1 << 0,
  RestorePrevious = 1 << 1,
  RestoreNext = 1 << 2,
  NoteSortKey = 1 << 3,
  NoteSortDirection = 1 << 4,
  PreviewLayoutType = 1 << 5,
  PreviewTileSize = 1 << 6,
  PreviewTileRatio = 1 << 7
}