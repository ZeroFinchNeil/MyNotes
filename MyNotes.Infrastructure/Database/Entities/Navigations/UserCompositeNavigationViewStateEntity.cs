using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MyNotes.Infrastructure.Database.Entities.Navigations;

internal class UserCompositeNavigationViewStateEntity : IDatabaseEntity<UserCompositeNavigationViewStateEntity>, IUserNavigationViewStateEntity
{
  [Key]
  public required Guid Id { get; init; }

  [ForeignKey(nameof(Id))]
  public UserNavigationEntity? Navigation { get; init; }
  
  public required bool IsExpanded { get; set; }

  public bool Equals(UserCompositeNavigationViewStateEntity? other) => other is not null && other.Id == Id;

  public override bool Equals(object? obj) => this.Equals(obj as UserCompositeNavigationViewStateEntity);
  public override int GetHashCode() => Id.GetHashCode();
}
