using MyNotes.Application.Contracts.Database.Dtos.Navigations.Common;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Creation;
using MyNotes.Common.Querying;
using MyNotes.Debugging.Attributes;
using MyNotes.Domain.ValueObjects;
using MyNotes.Infrastructure.Constants.Navigations;
using MyNotes.Infrastructure.Database.Entities.Navigations;
using MyNotes.Shared.Enums.Navigations;
using MyNotes.Shared.Enums.Notes;

namespace MyNotes.Infrastructure.Mappers;

[AssemblyLocal]
internal static class NavigationMappers
{
  public static NavigationEntity ToEntity(CreateNavigationDbRequestDto createDbRequestDto) => new()
  {
    Id = createDbRequestDto.Id.Value,
    Parent = createDbRequestDto.ParentId.Value,
    IsComposite = createDbRequestDto.IsComposite,
    Icon = createDbRequestDto.Icon,
    Title = createDbRequestDto.Title,
    Position = NavigationEntitySettings.TemporaryPosition,
    IsDeleted = false
  };

  public static CompositeNavigationViewStateDbResponseDto ToDto(CompositeNavigationViewStateEntity compositeViewStateEntity) => new()
  {
    Id = NavigationId.Create(compositeViewStateEntity.Id),
    IsExpanded = compositeViewStateEntity.IsExpanded
  };

  public static LeafNavigationViewStateDbResponseDto ToDto(LeafNavigationViewStateEntity leafViewStateEntity) => new()
  {
    Id = NavigationId.Create(leafViewStateEntity.Id),
    NoteSortKey = (NoteSortKey?)leafViewStateEntity.NoteSortKey,
    NoteSortDirection = (SortDirection?)leafViewStateEntity.NoteSortDirection,
    PreviewLayoutType = (PreviewLayoutType?)leafViewStateEntity.PreviewLayoutType,
    PreviewTileSize = (PreviewTileSize?)leafViewStateEntity.PreviewTileSize,
    PreviewTileRatio = (PreviewTileRatio?)leafViewStateEntity.PreviewTileRatio,
  };

  public static NavigationDbResponseDto ToDto(NavigationEntity entity) => new()
  {
    Id = NavigationId.Create(entity.Id),
    Parent = NavigationId.Create(entity.Parent),
    IsComposite = entity.IsComposite,
    Icon = entity.Icon,
    Title = entity.Title,
    IsDeleted = entity.IsDeleted,
    Position = entity.Position
  };

  public static NavigationBundleDbResponseDto BundleDbDto(NavigationDbResponseDto navigationDbResponseDto, NavigationViewStateDbResponseDto viewStateDbResponseDto) => new(navigationDbResponseDto, viewStateDbResponseDto);
}

internal static class NavigationMappingExtensions
{
  extension(NavigationEntity entity)
  {
    public NavigationDbResponseDto ToDto() => NavigationMappers.ToDto(entity);
  }

  extension(CompositeNavigationViewStateEntity compositeViewStateEntity)
  {
    public CompositeNavigationViewStateDbResponseDto ToDto() => NavigationMappers.ToDto(compositeViewStateEntity);
  }

  extension(LeafNavigationViewStateEntity leafViewStateEntity)
  {
    public LeafNavigationViewStateDbResponseDto ToDto() => NavigationMappers.ToDto(leafViewStateEntity);
  }
}