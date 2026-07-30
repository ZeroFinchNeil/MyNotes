using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Domain.Navigations;
using MyNotes.Models.Navigations;
using MyNotes.Templates;

namespace MyNotes.Common.Mappers;

internal static class NavigationMappers
{
  public static NavigationUserNode ToModel(NavigationTreeNodeDto dto, NavigationUserCompositeNode parentNode) => dto switch
  {
    CompositeNavigationTreeNodeDto compositeDto => ToCompositeNode(compositeDto, parentNode),
    LeafNavigationTreeNodeDto leafDto => ToLeafNode(leafDto, parentNode),
    _ => throw new InvalidOperationException($"지원하지 않는 navigation DTO 타입입니다: {dto.GetType().Name}")
  };

  private static NavigationUserCompositeNode ToCompositeNode(CompositeNavigationTreeNodeDto compositeDto, NavigationUserCompositeNode parentNode)
  {
    NavigationUserCompositeNode compositeNode = compositeDto.Id == NavigationId.UserRoot
      ? NavigationUserRootNode.Instance
      : new NavigationUserCompositeNode()
      {
        Id = compositeDto.Id,
        Parent = parentNode,
        Icon = (Icon)compositeDto.Icon,
        Title = compositeDto.Title,
        IsExpanded = ((CompositeNavigationViewStateDto)compositeDto.ViewStateDto).IsExpanded
      };
    foreach (var childDto in compositeDto.Children)
    {
      if (childDto.IsDeleted)
      {
        continue;
      }
      compositeNode.ChildNodes.Add(ToModel(childDto, compositeNode));
    }
    return compositeNode;
  }

  private static NavigationUserLeafNode ToLeafNode(LeafNavigationTreeNodeDto leafDto, NavigationUserCompositeNode parentNode)
  {
    var viewStateDto = (LeafNavigationViewStateDto)leafDto.ViewStateDto;
    return new()
    {
      Id = leafDto.Id,
      Parent = parentNode,
      Icon = (Icon)leafDto.Icon,
      Title = leafDto.Title,
      NoteSortKey = viewStateDto.NoteSortKey,
      NoteSortDirection = viewStateDto.NoteSortDirection,
      PreviewLayoutType = viewStateDto.PreviewLayoutType,
      PreviewTileSize = viewStateDto.PreviewTileSize,
      PreviewTileRatio = viewStateDto.PreviewTileRatio
    };
  }

  public static NavigationUserNode ToModel(NavigationDto dto, NavigationUserCompositeNode parentNode)
  {
    NavigationViewStateDto viewStateDto = dto.ViewStateDto;
    if (dto.IsComposite && viewStateDto is CompositeNavigationViewStateDto compositeViewStateDto)
    {
      return new NavigationUserCompositeNode()
      {
        Id = dto.Id,
        Parent = parentNode,
        Icon = (Icon)dto.Icon,
        Title = dto.Title,
        IsExpanded = compositeViewStateDto.IsExpanded,
      };
    }

    if (!dto.IsComposite && viewStateDto is LeafNavigationViewStateDto leafViewStateDto)
    {
      return new NavigationUserLeafNode()
      {
        Id = dto.Id,
        Parent = parentNode,
        Icon = (Icon)dto.Icon,
        Title = dto.Title,
        NoteSortKey = leafViewStateDto.NoteSortKey,
        NoteSortDirection = leafViewStateDto.NoteSortDirection,
        PreviewLayoutType = leafViewStateDto.PreviewLayoutType,
        PreviewTileSize = leafViewStateDto.PreviewTileSize,
        PreviewTileRatio = leafViewStateDto.PreviewTileRatio,
      };
    }

    throw new InvalidOperationException();
  }
}

internal static class NavigationMappingExtensions
{

}