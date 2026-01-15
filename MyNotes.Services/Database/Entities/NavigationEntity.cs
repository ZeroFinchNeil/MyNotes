using System;
using System.ComponentModel.DataAnnotations;

namespace MyNotes.Services.Database.Entities;

internal sealed class NavigationEntity : IEquatable<NavigationEntity>
{
  [Key]
  public required Guid Id { get; init; }

  public required string Title { get; set; }

  public required short Icon { get; set; }

  public required Guid Parent { get; set; }

  public required int Position { get; set; }

  public required bool IsComposite { get; init; }

  public required bool IsExpanded { get; set; }

  public required bool IsDeleted { get; set; }

  public Guid? RestorePrevious { get; set; }

  public Guid? RestoreNext { get; set; }

  public int? NoteSortKey { get; set; }

  public int? NoteSortDirection { get; set; }

  public int? PreviewLayoutType { get; set; }

  public int? PreviewTileSize { get; set; }

  public int? PreviewTileRatio { get; set; }

  public override string ToString() => $"NavigationEntity {{ Id: {Id}, Title: {Title} }}";

  public bool Equals(NavigationEntity? other) => other is not null && other.Id == Id;

  public override bool Equals(object? obj) => this.Equals(obj as NavigationEntity);
  public override int GetHashCode() => Id.GetHashCode();
}
