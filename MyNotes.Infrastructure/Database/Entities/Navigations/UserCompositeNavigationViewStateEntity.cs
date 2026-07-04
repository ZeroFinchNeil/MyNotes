using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyNotes.Infrastructure.Database.Entities.Navigations;

internal class UserCompositeNavigationViewStateEntity : IUserNavigationViewStateEntity<UserCompositeNavigationViewStateEntity>, IDatabaseEntity<UserCompositeNavigationViewStateEntity>
{
  [Key]
  public required Guid Id { get; init; }

  [ForeignKey(nameof(Id))]
  public UserNavigationEntity? Navigation { get; init; }

  public required bool IsExpanded { get; set; }

  public bool Equals(UserCompositeNavigationViewStateEntity? other) => other is not null && other.Id == Id;

  public override bool Equals(object? obj) => this.Equals(obj as UserCompositeNavigationViewStateEntity);
  public override int GetHashCode() => Id.GetHashCode();

  public static UserCompositeNavigationViewStateEntity CreateDefault(Guid id) => new()
  {
    Id = id,
    IsExpanded = true
  };
}
