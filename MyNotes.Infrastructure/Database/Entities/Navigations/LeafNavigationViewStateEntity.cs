using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyNotes.Infrastructure.Database.Entities.Navigations;

internal class LeafNavigationViewStateEntity : IDatabaseEntity<LeafNavigationViewStateEntity>, INavigationViewStateEntity
{
  [Key, Required]
  public required Guid Id { get; init; }

  [Required]
  public required int NoteSortKey { get; init; }

  [Required]
  public required int NoteSortDirection { get; init; }

  [Required]
  public required int PreviewLayoutType { get; init; }

  [Required]
  public required int PreviewTileSize { get; init; }

  [Required]
  public required int PreviewTileRatio { get; init; }

  public bool Equals(LeafNavigationViewStateEntity? other) => other is not null && other.Id == Id;

  public override bool Equals(object? obj) => this.Equals(obj as LeafNavigationViewStateEntity);
  public override int GetHashCode() => Id.GetHashCode();
}
