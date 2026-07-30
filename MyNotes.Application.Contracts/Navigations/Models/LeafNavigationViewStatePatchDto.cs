using DotNext;

using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Querying.Models;

namespace MyNotes.Application.Contracts.Navigations.Models;

internal sealed record LeafNavigationViewStatePatchDto : NavigationViewStatePatchDto
{
  public Optional<NoteSortKey> NoteSortKey { get; init; }

  public Optional<SortDirection> NoteSortDirection { get; init; }

  public Optional<PreviewLayoutType> PreviewLayoutType { get; init; }

  public Optional<PreviewTileSize> PreviewTileSize { get; init; }

  public Optional<PreviewTileRatio> PreviewTileRatio { get; init; }

  public override bool IsEmpty => this is
  {
    NoteSortKey.IsUndefined: true,
    NoteSortDirection.IsUndefined: true,
    PreviewLayoutType.IsUndefined: true,
    PreviewTileSize.IsUndefined: true,
    PreviewTileRatio.IsUndefined: true
  };
}