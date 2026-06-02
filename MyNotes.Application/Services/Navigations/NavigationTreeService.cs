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
  }
  #endregion

  private UserNavigationAppResponseDto ConvertTreeElement(NavigationTreeElement treeElement) => treeElement.Dto is UserCompositeNavigationAppResponseDto composite
    ? composite with { Children = [.. treeElement.Children.Select(ConvertTreeElement)] }
    : treeElement.Dto;

  public async Task<UserCompositeNavigationAppResponseDto> BuildNavigationTreeAsync()
  {
    var userNavigationDbDtos = await NavigationRepository.GetAllUserNavigationsAsync();
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