using System;

using MyNotes.Application.Contracts.Models.Navigations;
using MyNotes.Common.Querying;
using MyNotes.Debugging.Attributes;
using MyNotes.Domain.ValueObjects;
using MyNotes.Infrastructure.Database.Entities.Navigations;
using MyNotes.Shared.Enums.Navigations;
using MyNotes.Shared.Enums.Notes;

namespace MyNotes.Infrastructure.Mappers;

[AssemblyLocal]
internal static class NavigationMappers
{
  public static NavigationEntity ToEntity(NavigationDto navigationDto, int position) => new()
  {
    Id = navigationDto.Id.Value,
    Parent = navigationDto.ParentId.Value,
    IsComposite = navigationDto.IsComposite,
    Icon = navigationDto.Icon,
    Title = navigationDto.Title,
    Position = position,
    IsDeleted = navigationDto.IsDeleted
  };

  public static CompositeNavigationViewStateEntity ToEntity(CompositeNavigationViewStateDto compositeDto) => new()
  {
    Id = compositeDto.Id.Value,
    IsExpanded = compositeDto.IsExpanded
  };

  public static LeafNavigationViewStateEntity ToEntity(LeafNavigationViewStateDto leafDto) => new()
  {
    Id = leafDto.Id.Value,
    NoteSortKey = (int?)leafDto.NoteSortKey,
    NoteSortDirection = (int?)leafDto.NoteSortDirection,
    PreviewLayoutType = (int?)leafDto.PreviewLayoutType,
    PreviewTileSize = (int?)leafDto.PreviewTileSize,
    PreviewTileRatio = (int?)leafDto.PreviewTileRatio
  };

  public static CompositeNavigationViewStateDto ToDto(CompositeNavigationViewStateEntity compositeViewStateEntity) => new()
  {
    Id = NavigationId.Create(compositeViewStateEntity.Id),
    IsExpanded = compositeViewStateEntity.IsExpanded
  };

  public static LeafNavigationViewStateDto ToDto(LeafNavigationViewStateEntity leafViewStateEntity) => new()
  {
    Id = NavigationId.Create(leafViewStateEntity.Id),
    NoteSortKey = (NoteSortKey?)leafViewStateEntity.NoteSortKey,
    NoteSortDirection = (SortDirection?)leafViewStateEntity.NoteSortDirection,
    PreviewLayoutType = (PreviewLayoutType?)leafViewStateEntity.PreviewLayoutType,
    PreviewTileSize = (PreviewTileSize?)leafViewStateEntity.PreviewTileSize,
    PreviewTileRatio = (PreviewTileRatio?)leafViewStateEntity.PreviewTileRatio,
  };

  public static NavigationViewStateDto ToDto(INavigationViewStateEntity viewStateEntity) => viewStateEntity switch
  {
    LeafNavigationViewStateEntity leafEntity => ToDto(leafEntity),
    CompositeNavigationViewStateEntity compositeEntity => ToDto(compositeEntity),
    _ => throw new InvalidOperationException()
  };

  public static NavigationDto ToDto(NavigationEntity entity, INavigationViewStateEntity viewStateEntity) => new()
  {
    Id = NavigationId.Create(entity.Id),
    ParentId = NavigationId.Create(entity.Parent),
    IsComposite = entity.IsComposite,
    Icon = entity.Icon,
    Title = entity.Title,
    IsDeleted = entity.IsDeleted,
    ViewStateDto = ToDto(viewStateEntity)
  };
}