using MyNotes.Application.Dtos.Navigations.Common;
using MyNotes.Domain.ValueObjects;
using MyNotes.Models.Navigations;

namespace MyNotes.Mappers;

internal static class NavigationMappers
{
  public static NavigationUserNode ToModel(UserNavigationBundleAppResponseDto dto, NavigationUserCompositeNode parentNode) => dto switch
  {
    UserCompositeNavigationBundleAppResponseDto compositeDto => ToCompositeNode(compositeDto, parentNode),
    UserLeafNavigationBundleAppResponseDto leafDto => ToLeafNode(leafDto, parentNode),
    _ => throw new InvalidOperationException($"지원하지 않는 navigation DTO 타입입니다: {dto.GetType().Name}")
  };

  private static NavigationUserCompositeNode ToCompositeNode(UserCompositeNavigationBundleAppResponseDto compositeDto, NavigationUserCompositeNode parentNode)
  {
    
    NavigationUserCompositeNode compositeNode = compositeDto.Id == NavigationId.UserRoot
      ? NavigationUserRootNode.Instance
      : new NavigationUserCompositeNode()
      {
        Id = compositeDto.Id,
        Parent = parentNode,
        Icon = compositeDto.UserNavigationDto.Icon,
        Title = compositeDto.UserNavigationDto.Title,
        IsExpanded = compositeDto.ViewStateDto.IsExpanded
      };
    foreach (var childDto in compositeDto.Children)
    {
      if (childDto.UserNavigationDto.IsDeleted)
      {
        continue;
      }
      compositeNode.ChildNodes.Add(ToModel(childDto, compositeNode));
    }
    return compositeNode;
  }

  private static NavigationUserLeafNode ToLeafNode(UserLeafNavigationBundleAppResponseDto leafDto, NavigationUserCompositeNode parentNode) => new()
  {
    Id = leafDto.UserNavigationDto.Id,
    Parent = parentNode,
    Icon = leafDto.UserNavigationDto.Icon,
    Title = leafDto.UserNavigationDto.Title,
    NoteSortKey = leafDto.ViewStateDto.NoteSortKey,
    NoteSortDirection = leafDto.ViewStateDto.NoteSortDirection,
    PreviewLayoutType = leafDto.ViewStateDto.PreviewLayoutType,
    PreviewTileSize = leafDto.ViewStateDto.PreviewTileSize,
    PreviewTileRatio = leafDto.ViewStateDto.PreviewTileRatio
  };
}

internal static class NavigationMappingExtensions
{

}