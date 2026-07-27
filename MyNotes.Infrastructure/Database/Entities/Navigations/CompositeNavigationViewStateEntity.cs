using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyNotes.Infrastructure.Database.Entities.Navigations;

internal class CompositeNavigationViewStateEntity : IDatabaseEntity<CompositeNavigationViewStateEntity>, INavigationViewStateEntity
{
  [Key, Required]
  public required Guid Id { get; init; }

  [Required]
  public required bool IsExpanded { get; set; }

  public bool Equals(CompositeNavigationViewStateEntity? other) => other is not null && other.Id == Id;

  public override bool Equals(object? obj) => this.Equals(obj as CompositeNavigationViewStateEntity);
  public override int GetHashCode() => Id.GetHashCode();
}
