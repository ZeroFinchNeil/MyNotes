using System;
using System.ComponentModel.DataAnnotations;

namespace MyNotes.Infrastructure.Database.Entities.Navigations;

internal sealed class NavigationEntity : IDatabaseEntity<NavigationEntity>, IComparable<NavigationEntity>
{
  [Key, Required]
  public required Guid Id { get; init; }

  [Required]
  public required Guid Parent { get; set; }

  [Required]
  public required bool IsComposite { get; init; }

  [Required]
  public required int Icon { get; set; }

  [Required]
  public required string Title { get; set; }

  [Required]
  public required int Position { get; set; }

  [Required]
  public required bool IsDeleted { get; set; }

  public override string ToString() => $"NavigationEntity {{ Id: {Id}, Title: {Title} }}";

  public bool Equals(NavigationEntity? other) => other is not null && other.Id == Id;

  public override bool Equals(object? obj) => this.Equals(obj as NavigationEntity);
  public override int GetHashCode() => Id.GetHashCode();

  public int CompareTo(NavigationEntity? other) => other is null ? 1 : this.Position.CompareTo(other.Position);
}
