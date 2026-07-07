using System;

using MyNotes.Application.Contracts.Database.Dtos.Navigations.Common;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Creation;
using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Debugging.Attributes;
using MyNotes.Domain.ValueObjects;
using MyNotes.Infrastructure.Constants.Navigations;
using MyNotes.Infrastructure.Database.Entities.Navigations;

namespace MyNotes.Infrastructure.Mappers;

[AssemblyLocal]
internal static class UserNavigationMappers
{
  public static UserNavigationEntity ToEntity(CreateUserNavigationDbRequestDto createUserNavigationDbRequestDto) => new()
  {
    Id = createUserNavigationDbRequestDto.Id.Value,
    Parent = createUserNavigationDbRequestDto.ParentId.Value,
    IsComposite = createUserNavigationDbRequestDto.IsComposite,
    Icon = (short)createUserNavigationDbRequestDto.Icon,
    Title = createUserNavigationDbRequestDto.Title,
    Position = UserNavigationEntitySettings.TemporaryPosition,
    IsDeleted = false
  };

  public static UserCompositeNavigationViewStateDbResponseDto ToDto(UserCompositeNavigationViewStateEntity userCompositeNavigationViewStateEntity) => new()
  {
    Id = NavigationId.Create(userCompositeNavigationViewStateEntity.Id),
    IsExpanded = userCompositeNavigationViewStateEntity.IsExpanded
  };

  public static UserLeafNavigationViewStateDbResponseDto ToDto(UserLeafNavigationViewStateEntity userLeafNavigationViewStateEntity) => new()
  {
    Id = NavigationId.Create(userLeafNavigationViewStateEntity.Id),
    NoteSortKey = userLeafNavigationViewStateEntity.NoteSortKey,
    NoteSortDirection = userLeafNavigationViewStateEntity.NoteSortDirection,
    PreviewLayoutType = userLeafNavigationViewStateEntity.PreviewLayoutType,
    PreviewTileSize = userLeafNavigationViewStateEntity.PreviewTileSize,
    PreviewTileRatio = userLeafNavigationViewStateEntity.PreviewTileRatio,
  };

  public static UserNavigationDbResponseDto ToDto(UserNavigationEntity userNavigationEntity) => new()
  {
    Id = NavigationId.Create(userNavigationEntity.Id),
    Parent = NavigationId.Create(userNavigationEntity.Parent),
    IsComposite= userNavigationEntity.IsComposite,
    Icon = userNavigationEntity.Icon,
    Title = userNavigationEntity.Title,
    IsDeleted = userNavigationEntity.IsDeleted,
    Position = userNavigationEntity.Position
  };

  public static UserNavigationBundleDbResponseDto BundleDbDto(UserNavigationDbResponseDto userNavigationDbResponseDto, UserNavigationViewStateDbResponseDto userNavigationViewStateDbResponseDto) => new(userNavigationDbResponseDto, userNavigationViewStateDbResponseDto);
}

internal static class UserNavigationMappingExtensions
{
  extension(UserNavigationEntity entity)
  {
    public UserNavigationDbResponseDto ToDto() => UserNavigationMappers.ToDto(entity);
  }

  extension(UserCompositeNavigationViewStateEntity compositeViewStateEntity)
  {
    public UserCompositeNavigationViewStateDbResponseDto ToDto() => UserNavigationMappers.ToDto(compositeViewStateEntity);
  }

  extension(UserLeafNavigationViewStateEntity leafViewStateEntity)
  {
    public UserLeafNavigationViewStateDbResponseDto ToDto() => UserNavigationMappers.ToDto(leafViewStateEntity);
  }
}