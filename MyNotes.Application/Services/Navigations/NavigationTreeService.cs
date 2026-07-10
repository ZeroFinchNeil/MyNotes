using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Application.Dtos.Navigations.Common;
using MyNotes.Application.Mappers;
using MyNotes.Domain.ValueObjects;
using MyNotes.Shared.Constants;

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

  private UserNavigationBundleAppResponseDto ConvertTreeElement(NavigationTreeElement treeElement) => treeElement.BundleDto is UserCompositeNavigationBundleAppResponseDto compositeBundleDto
    ? compositeBundleDto with { Children = [.. treeElement.Children.Select(ConvertTreeElement)] }
    : treeElement.BundleDto;

  public async Task<UserCompositeNavigationBundleAppResponseDto> BuildNavigationTreeAsync(CancellationToken cancellationToken = default)
  {
    var dbBundleResponseDto = await NavigationRepository.GetAllUserNavigationsAsync(cancellationToken);

    var treeElementsById = dbBundleResponseDto.ToDictionary(
      bundleDbDto => bundleDbDto.UserNavigationDto.Id,
      bundleDbDto => new NavigationTreeElement()
      {
        BundleDto = UserNavigationMappers.ToAppDto(bundleDbDto),
        Position = bundleDbDto.UserNavigationDto.Position
      });

    UserCompositeNavigationBundleAppResponseDto rootAppDto = new(
      userNavigationDto: new UserCompositeNavigationAppResponseDto()
      {
        Id = NavigationId.UserRoot,
        Parent = NavigationId.Empty,
        Title = AppStrings.UserRootNavigationName,
        Icon = Templates.Icon.System_Library,
        IsDeleted = false,
      },
      viewStateDto: new UserCompositeNavigationViewStateAppResponseDto()
      {
        Id = NavigationId.UserRoot,
        IsExpanded = true
      },
      children: []
    );

    NavigationTreeElement rootTreeElement = new() { BundleDto = rootAppDto, Position = 0 };
    treeElementsById.Add(NavigationId.UserRoot, rootTreeElement);

    List<NavigationTreeElement> omittedTreeElements = new();

    foreach (var treeElement in treeElementsById.Values)
    {
      if (treeElement == rootTreeElement)
      {
        continue;
      }

      if (treeElementsById.TryGetValue(treeElement.BundleDto.UserNavigationDto.Parent, out var parentTreeElement))
      {
        parentTreeElement.Children.Add(treeElement);
      }
      else
      {
        omittedTreeElements.Add(treeElement);
      }
    }

    return rootAppDto with { Children = [.. ((UserCompositeNavigationBundleAppResponseDto)ConvertTreeElement(rootTreeElement)).Children, .. omittedTreeElements.Select(ConvertTreeElement)] };
  }

  private class NavigationTreeElement : IComparable<NavigationTreeElement>
  {
    public required UserNavigationBundleAppResponseDto BundleDto { get; init; }

    public SortedSet<NavigationTreeElement> Children { get; } = new();

    public required int Position { get; init; }

    public int CompareTo(NavigationTreeElement? other)
    {
      if (other is null)
      {
        return 1;
      }

      int cmp = Position.CompareTo(other.Position);
      return cmp != 0 ? cmp : BundleDto.Id.Value.CompareTo(other.BundleDto.Id.Value);
    }
  }
}