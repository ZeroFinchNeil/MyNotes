using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Application.Dtos.Navigations;
using MyNotes.Application.Mappers;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Services.Navigations;

internal sealed partial class NavigationTreeService
{
  private readonly INavigationRepository NavigationRepository;

  #region Object Lifetime Management
  public NavigationTreeService(INavigationRepository navigationRepository)
  {
    NavigationRepository = navigationRepository;
    _ = BuildNavigationTreeAsync();
  }
  #endregion

  private UserNavigationAppResponseDto ConvertTreeElement(NavigationTreeElement treeElement) => treeElement.Dto is UserCompositeNavigationAppResponseDto composite
    ? composite with { Children = [.. treeElement.Children.Select(ConvertTreeElement)] }
    : treeElement.Dto;

  public async Task<UserCompositeNavigationAppResponseDto> BuildNavigationTreeAsync()
  {
    var userNavigationDbDtos = await NavigationRepository.GetUserNavigationsAsync();
    var treeElements = userNavigationDbDtos.ToDictionary(dbDto => dbDto.Id, dto => new NavigationTreeElement() { Dto = UserNavigationMappers.ToAppDto(dto) });

    UserCompositeNavigationAppResponseDto rootAppDto = new UserCompositeNavigationAppResponseDto()
    {
      Id = NavigationId.UserRoot,
      Parent = NavigationId.Empty,
      Title = "Root",
      Icon = Templates.Icon.System_Library,
      Position = 0,
      Children = [],
      IsDeleted = false,
      IsExpanded = true
    };

    NavigationTreeElement rootTreeElement = new() { Dto = rootAppDto };
    treeElements.Add(NavigationId.UserRoot, rootTreeElement);

    List<NavigationTreeElement> omittedTreeElements = new();
    foreach (var treeElement in treeElements.Values)
    {
      if (treeElement == rootTreeElement)
      {
        continue;
      }

      if (treeElements.TryGetValue(treeElement.Dto.Parent, out var parentTreeElement))
      {
        parentTreeElement.Children.Add(treeElement);
      }
      else
      {
        omittedTreeElements.Add(treeElement);
      }
    }

    return rootAppDto with { Children = [.. ((UserCompositeNavigationAppResponseDto)ConvertTreeElement(rootTreeElement)).Children, .. omittedTreeElements.Select(ConvertTreeElement)] };
  }

  private class NavigationTreeElement : IComparable<NavigationTreeElement>
  {
    public required UserNavigationAppResponseDto Dto { get; init; }

    public SortedSet<NavigationTreeElement> Children { get; } = new();

    public int CompareTo(NavigationTreeElement? other)
    {
      if (other is null)
      {
        return 1;
      }

      int cmp = Dto.Position.CompareTo(other.Dto.Position);
      return cmp != 0 ? cmp : Dto.Id.Value.CompareTo(other.Dto.Id.Value);
    }
  }
}

//var nodes = entities
//  .Select<NavigationEntity, NavigationUserNode>(e => e.IsComposite
//    ? new NavigationUserCompositeNode()
//    {
//      Id = NavigationId.Create(e.Id),
//      Parent = null!,
//      Icon = (Icon)e.Icon,
//      Title = e.Title,
//      Position = e.Position,
//      IsExpanded = e.IsExpanded
//    }
//  : new NavigationUserLeafNode()
//  {
//    Id = NavigationId.Create(e.Id),
//    Parent = null!,
//    Icon = (Icon)e.Icon,
//    Title = e.Title,
//    Position = e.Position,
//    NoteSortKey = e.NoteSortKey.AsEnum<NoteSortKey>(),
//    NoteSortDirection = e.NoteSortDirection.AsEnum<SortDirection>(),
//    PreviewLayoutType = e.PreviewLayoutType.AsEnum<PreviewLayoutType>(),
//    PreviewTileSize = e.PreviewTileSize.AsEnum<PreviewTileSize>(),
//    PreviewTileRatio = e.PreviewTileRatio.AsEnum<PreviewTileRatio>(),
//  })
// .ToDictionary(n => n.Id.Value);

//HashSet<NavigationEntity> omissions = [.. entities];

//nodes.Add(NavigationService.UserRootNavigation.Id.Value, NavigationService.UserRootNavigation);

//var families = entities
//  .GroupBy(e => e.Parent)
//  .ToDictionary(g => g.Key, g => new SortedSet<NavigationEntity>(g, Comparer<NavigationEntity>.Create((x, y) => x.Position.CompareTo(y.Position))));

//foreach (var family in families)
//{
//  if (nodes.TryGetValue(family.Key, out var parent) && parent is NavigationUserCompositeNode compositeNode)
//  {
//    foreach (var childEntity in family.Value)
//    {
//      if (nodes.TryGetValue(childEntity.Id, out var childNode))
//      {
//        if (!childEntity.IsDeleted)
//        {
//          compositeNode.ChildNodes.Add(childNode);
//        }

//        omissions.Remove(childEntity);
//      }
//    }
//  }
//}

//foreach (var node in nodes.Values)
//{
//  node.PropertyChanged += UserNode_PropertyChanged;
//}

// 내비게이션 트리에 들어가지 못한 누락된 내비게이션 처리
//foreach (var omission in omissions)
//{
//  if (nodes.TryGetValue(omission.Parent, out var parentNode)
//    && parentNode is NavigationUserCompositeNode compositeNode
//    && nodes.TryGetValue(omission.Id, out var omitNode))
//  {
//    var childNodes = compositeNode.ChildNodes;
//    var pivot = childNodes.FirstOrDefault(n => n.Position > omitNode.Position);
//    int index = pivot is null
//      ? childNodes.Count == 0 || omitNode.Position <= childNodes[0].Position ? 0 : childNodes.Count
//      : childNodes.IndexOf(pivot);
//    compositeNode.ChildNodes.Insert(index, omitNode);
//  }
//}