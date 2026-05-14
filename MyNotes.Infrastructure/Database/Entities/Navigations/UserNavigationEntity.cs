using System;
using System.ComponentModel.DataAnnotations;

namespace MyNotes.Infrastructure.Database.Entities.Navigations;

internal sealed class UserNavigationEntity : IDatabaseEntity<UserNavigationEntity>
{
  [Key]
  public required Guid Id { get; init; }

  public required Guid Parent { get; set; }

  public required bool IsComposite { get; init; }

  public required short Icon { get; set; }

  public required string Title { get; set; }

  public required int Position { get; set; }

  public required bool IsDeleted { get; set; }

  public override string ToString() => $"NavigationEntity {{ Id: {Id}, Title: {Title} }}";

  public bool Equals(UserNavigationEntity? other) => other is not null && other.Id == Id;

  public override bool Equals(object? obj) => this.Equals(obj as UserNavigationEntity);
  public override int GetHashCode() => Id.GetHashCode();
}
