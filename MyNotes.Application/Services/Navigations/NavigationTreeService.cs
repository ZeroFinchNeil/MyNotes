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

  private NavigationBundleAppResponseDto ConvertTreeElement(NavigationTreeElement treeElement) => treeElement.BundleDto is CompositeNavigationBundleAppResponseDto compositeBundleDto
    ? compositeBundleDto with { Children = [.. treeElement.Children.Select(ConvertTreeElement)] }
    : treeElement.BundleDto;

  public async Task<CompositeNavigationBundleAppResponseDto> BuildNavigationTreeAsync(CancellationToken cancellationToken = default)
  {
    var dbBundleResponseDto = await NavigationRepository.GetAllNavigationsAsync(cancellationToken);

    var treeElementsById = dbBundleResponseDto.ToDictionary(
      bundleDbDto => bundleDbDto.NavigationDto.Id,
      bundleDbDto => new NavigationTreeElement()
      {
        BundleDto = NavigationMappers.ToAppDto(bundleDbDto),
        Position = bundleDbDto.NavigationDto.Position
      });

    CompositeNavigationBundleAppResponseDto rootAppDto = new(
      navigationDto: new CompositeNavigationAppResponseDto()
      {
        Id = NavigationId.UserRoot,
        Parent = NavigationId.Empty,
        Title = AppStrings.UserRootNavigationName,
        Icon = Templates.Icon.System_Library,
        IsDeleted = false,
      },
      viewStateDto: new CompositeNavigationViewStateAppResponseDto()
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

      if (treeElementsById.TryGetValue(treeElement.BundleDto.NavigationDto.Parent, out var parentTreeElement))
      {
        parentTreeElement.Children.Add(treeElement);
      }
      else
      {
        omittedTreeElements.Add(treeElement);
      }
    }

    return rootAppDto with { Children = [.. ((CompositeNavigationBundleAppResponseDto)ConvertTreeElement(rootTreeElement)).Children, .. omittedTreeElements.Select(ConvertTreeElement)] };
  }

  private class NavigationTreeElement : IComparable<NavigationTreeElement>
  {
    public required NavigationBundleAppResponseDto BundleDto { get; init; }

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