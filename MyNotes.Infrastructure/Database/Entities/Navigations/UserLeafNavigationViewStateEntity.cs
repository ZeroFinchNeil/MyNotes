using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MyNotes.Infrastructure.Database.Entities.Navigations;

internal class UserLeafNavigationViewStateEntity : IDatabaseEntity<UserLeafNavigationViewStateEntity>, IUserNavigationViewStateEntity
{
  [Key]
  public required Guid Id { get; init; }

  [ForeignKey(nameof(Id))]
  public UserNavigationEntity? Navigation { get; init; }

  public Guid? RestorePrevious { get; set; }

  public Guid? RestoreNext { get; set; }

  public int? NoteSortKey { get; set; }

  public int? NoteSortDirection { get; set; }

  public int? PreviewLayoutType { get; set; }

  public int? PreviewTileSize { get; set; }

  public int? PreviewTileRatio { get; set; }

  public bool Equals(UserLeafNavigationViewStateEntity? other) => other is not null && other.Id == Id;

  public override bool Equals(object? obj) => this.Equals(obj as UserLeafNavigationViewStateEntity);
  public override int GetHashCode() => Id.GetHashCode();
}
