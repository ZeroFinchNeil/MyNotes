using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyNotes.Infrastructure.Database.Entities.Navigations;

internal class UserLeafNavigationViewStateEntity : IDatabaseEntity<UserLeafNavigationViewStateEntity>, IUserNavigationViewStateEntity<UserLeafNavigationViewStateEntity>
{
  [Key, Required]
  public required Guid Id { get; init; }

  [ForeignKey(nameof(Id)), Required]
  public UserNavigationEntity? Navigation { get; init; }

  public required int? NoteSortKey { get; init; }

  public required int? NoteSortDirection { get; init; }

  public required int? PreviewLayoutType { get; init; }

  public required int? PreviewTileSize { get; init; }

  public required int? PreviewTileRatio { get; init; }

  public bool Equals(UserLeafNavigationViewStateEntity? other) => other is not null && other.Id == Id;

  public override bool Equals(object? obj) => this.Equals(obj as UserLeafNavigationViewStateEntity);
  public override int GetHashCode() => Id.GetHashCode();

  public static UserLeafNavigationViewStateEntity CreateDefault(Guid id) => new()
  {
    Id = id,
    NoteSortKey = null,
    NoteSortDirection = null,
    PreviewLayoutType = null,
    PreviewTileSize = null,
    PreviewTileRatio = null
  };
}
