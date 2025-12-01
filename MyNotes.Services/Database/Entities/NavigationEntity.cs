using System;
using System.ComponentModel.DataAnnotations;

namespace MyNotes.Services.Database.Entities;

internal sealed class NavigationEntity : IEquatable<NavigationEntity>
{
  [Key]
  public required Guid Id { get; init; }

  public required string Title { get; set; }

  public required Guid Parent { get; set; }

  public required Guid Next { get; set; }

  public override string ToString() => string.Format(
    """
    {0,8} | {1}
    {2,8} | {3}
    {4,8} | {5}
    {6,8} | {7}
    """,
    "ID", Id,
    "Title", Title,
    "Parent", Parent,
    "Next", Next);

  public bool Equals(NavigationEntity? other) => other is not null && other.Id == Id;

  public override bool Equals(object? obj) => this.Equals(obj as NavigationEntity);
  public override int GetHashCode() => Id.GetHashCode();
}
