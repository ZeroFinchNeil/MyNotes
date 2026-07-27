using MyNotes.Application.Contracts.Models.Navigations;
using MyNotes.Application.Contracts.Persistence.Navigations;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Services.Navigations;

internal sealed partial class NavigationRetrievalService
{
  private readonly INavigationRepository NavigationRepository;

  public NavigationRetrievalService(INavigationRepository navigationRepository)
  {
    NavigationRepository = navigationRepository;
  }

  public async Task<CompositeNavigationTreeNodeDto> BuildNavigationTreeAsync(CancellationToken cancellationToken = default)
  {
    var orderedNavigations = await NavigationRepository.GetAllNavigationsInSiblingOrderAsync(cancellationToken);
    var childrenByParent = orderedNavigations.ToLookup(navigation => navigation.ParentId);

    return new()
    {
      Children = [.. childrenByParent[NavigationId.UserRoot].Select(BuildTreeNode)],
      Id = NavigationId.UserRoot,
      ParentId = NavigationId.Empty,
      Icon = default,
      Title = default!,
      IsDeleted = default,
      ViewStateDto = default!
    };

    NavigationTreeNodeDto BuildTreeNode(NavigationDto navigationDto) => navigationDto.IsComposite
      ? new CompositeNavigationTreeNodeDto()
      {
        Children = [.. childrenByParent[navigationDto.Id].Select(BuildTreeNode)],
        Id = navigationDto.Id,
        ParentId = navigationDto.ParentId,
        Icon = navigationDto.Icon,
        Title = navigationDto.Title,
        IsDeleted = navigationDto.IsDeleted,
        ViewStateDto = navigationDto.ViewStateDto
      }
      : new LeafNavigationTreeNodeDto()
      {
        Id = navigationDto.Id,
        ParentId = navigationDto.ParentId,
        Icon = navigationDto.Icon,
        Title = navigationDto.Title,
        IsDeleted = navigationDto.IsDeleted,
        ViewStateDto = navigationDto.ViewStateDto
      };
  }
}
