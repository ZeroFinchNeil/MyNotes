using System;

namespace MyNotes.Application.Contracts.Enums.Navigations;

[Flags]
internal enum LeafNavigationViewStateGetFields
{
  None = 0,
  NoteSortKey = 1 << 0,
  NoteSortDirection = 1 << 1,
  PreviewLayoutType = 1 << 2,
  PreviewTileSize = 1 << 3,
  PreviewTileRatio = 1 << 4
}
