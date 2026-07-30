using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Debugging.Attributes;
using MyNotes.Domain.Navigations;

namespace MyNotes.Application.Navigations;

[AssemblyLocal]
internal static class NavigationMappers
{
  public static NavigationDto ToDto(Navigation navigation, NavigationViewStateDto viewStateDto) =>
    (navigation.IsComposite && viewStateDto is CompositeNavigationViewStateDto) || (!navigation.IsComposite && viewStateDto is LeafNavigationViewStateDto)
      ? new()
      {
        Id = navigation.Id,
        ParentId = navigation.ParentId,
        IsComposite = navigation.IsComposite,
        Icon = navigation.Icon,
        Title = navigation.Title,
        IsDeleted = navigation.IsDeleted,
        ViewStateDto = viewStateDto
      }
      : throw new InvalidOperationException();
}

