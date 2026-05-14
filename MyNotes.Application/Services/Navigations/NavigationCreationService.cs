using MyNotes.Application.Contracts.Database.Dtos.Navigations;
using MyNotes.Application.Contracts.Database.Enums.Navigations;
using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Application.Dtos.Navigations;
using MyNotes.Common.Exceptions;
using MyNotes.Domain.Entities.Navigations;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Services.Navigations;

internal sealed partial class NavigationCreationService
{
  private readonly INavigationRepository NavigationRepository;
  public NavigationCreationService(INavigationRepository navigationRepository)
  {
    NavigationRepository = navigationRepository;
  }

  public async Task<UserNavigationAppResponseDto?> AddUserNavigationAsync(CreateUserNavigationAppRequestDto createUserNavigationAppRequestDto)
  {
    NavigationId insertTargetId = createUserNavigationAppRequestDto.InsertTargetId;
    NavigationInsertPosition insertPosition = createUserNavigationAppRequestDto.NavigationInsertPosition;
    NavigationId parentId = createUserNavigationAppRequestDto.ParentId;

    // Insert Target Navigation이 DB에 존재하는지 확인 후 Id와 Parent, IsComposite 속성 가져옴
    UserNavigationGetFields userNavigationGetFields = UserNavigationGetFields.Id | UserNavigationGetFields.Parent | UserNavigationGetFields.IsComposite;
    GetUserNavigationFieldValuesDbRequestDto getUserNavigationFieldsDbRequestDto = new()
    {
      UserNavigationGetFields = userNavigationGetFields,
      Id = insertTargetId
    };
    GetUserNavigationFieldValuesDbResponseDto getUserNavigationFieldsDbResponseDto = await NavigationRepository.GetUserNavigationFieldsAsync(getUserNavigationFieldsDbRequestDto);

    UserNavigationAppResponseDto? resultDto = null;

    // Application과 Infra DB의 Target Navigation 정보 일치 확인 후 새 Navigation 추가
    if (getUserNavigationFieldsDbResponseDto.UserNavigationGetFields.Equals(getUserNavigationFieldsDbRequestDto)
      && getUserNavigationFieldsDbResponseDto.Id == insertTargetId
      && getUserNavigationFieldsDbResponseDto.Parent is NavigationId targetParentId
      && getUserNavigationFieldsDbResponseDto.IsComposite is bool isTargetComposite)
    {
      switch (insertPosition)
      {
        case NavigationInsertPosition.Before or NavigationInsertPosition.After:
          if (parentId != targetParentId)
          {
            throw new InvalidStateException("추가하려는 Navigation과 Target Navigation의 Parent가 일치하지 않습니다.");
          }
          break;
        case NavigationInsertPosition.FirstChild or NavigationInsertPosition.LastChild:
          if (!isTargetComposite || parentId != insertTargetId)
          {
            throw new InvalidStateException("Composite이 아닌 Navigation에 자식 요소로 추가할 수 없습니다.");
          }
          break;
      }

      // DB에 있는 Navigation들과 일치하지 않는 Unique Id 생성 -> 새로운 Navigation의 Id로 사용
      NavigationId newNavigationId = await NavigationRepository.GenerateUniqueUserNavigationIdAsync();

      // UserNavigation Domain Entity로 변환하여 도메인 속성 유효성 검사
      UserNavigation userNavigation = new(newNavigationId, parentId, createUserNavigationAppRequestDto.IsComposite, (int)createUserNavigationAppRequestDto.Icon, createUserNavigationAppRequestDto.Title, false);

      UserNavigationDbAggregateResponseDto userNavigationDbAggregateResponseDto = await NavigationRepository.AddUserNavigationAsync(new CreateUserNavigationDbRequestDto()
      {
        Id = newNavigationId,
        InsertTargetId = insertTargetId,
        NavigationInsertPosition = insertPosition,
        IsComposite = userNavigation.IsComposite,
        Icon = userNavigation.Icon,
        Title = userNavigation.Title,
      });

      UserNavigationDbResponseDto userNavigationDbResponseDto = userNavigationDbAggregateResponseDto.UserNavigationDbResponseDto;
      UserNavigationViewStateDbResponseDto userNavigationViewStateDbResponseDto = userNavigationDbAggregateResponseDto.UserNavigationViewStateDbResponseDto;

      resultDto = createUserNavigationAppRequestDto.IsComposite
        ? new UserCompositeNavigationAppResponseDto()
        {
          Id = newNavigationId,
          Parent = insertTargetId,
          Icon = (Templates.Icon)userNavigationDbResponseDto.Icon,
          Title = userNavigationDbResponseDto.Title,
          Position = userNavigationDbResponseDto.Position,
          IsDeleted = false,
          Children = [],
          IsExpanded = true
        }
        : new UserLeafNavigationAppResponseDto()
        {
          Id = newNavigationId,
          Parent = insertTargetId,
          Icon = (Templates.Icon)userNavigationDbResponseDto.Icon,
          Title = userNavigationDbResponseDto.Title,
          Position = int.MaxValue,
          IsDeleted = false,
          NoteSortKey = null,
          NoteSortDirection = null,
          PreviewLayoutType = null,
          PreviewTileSize = null,
          PreviewTileRatio = null
        };

    }

    return resultDto;
  }

#if false
  // Navigation 인스턴스 생성 및 DB 테이블에 추가
  public async Task<NavigationUserNode?> AddUserNodeAsync(INavigationNode? targetNode, bool isCompositeNode, Icon icon, string title)
  {
    NavigationUserNode? beforeNode = targetNode switch
    {
      NavigationUserLeafNode leaf => leaf,
      NavigationUserCompositeNode composite => composite.ChildNodes.LastOrDefault(),
      _ => NavigationService.UserRootNavigation.ChildNodes.LastOrDefault()
    };

    NavigationUserCompositeNode parentNode = beforeNode is null
      ? targetNode switch
      {
        NavigationUserLeafNode leaf => leaf.Parent,
        NavigationUserCompositeNode composite => composite,
        _ => NavigationService.UserRootNavigation
      }
      : beforeNode.Parent;

    NavigationUserNode newNode = isCompositeNode
      ? new NavigationUserCompositeNode()
      {
        Id = NavigationId.NewId(),
        Parent = parentNode,
        Icon = icon,
        Title = title,
        Position = int.MaxValue,
        IsExpanded = true
      }
      : new NavigationUserLeafNode()
      {
        Id = NavigationId.NewId(),
        Parent = parentNode,
        Icon = icon,
        Title = title,
        Position = int.MaxValue,
      };

    await using var context = await DbContextFactory.CreateDbContextAsync();

    if (!await context.NavigationEntities.AnyAsync(e => e.Id == newNode.Id.Value))
    {
      int index = beforeNode is null ? parentNode.ChildNodes.Count : parentNode.ChildNodes.IndexOf(beforeNode) + 1;
      parentNode.ChildNodes.Insert(index, newNode);
      newNode.PropertyChanged += UserNode_PropertyChanged;

      NavigationEntity entity = new()
      {
        Id = newNode.Id.Value,
        Title = newNode.Title,
        Icon = (int)newNode.Icon,
        Parent = newNode.Parent.Id.Value,
        Position = newNode.Position,
        IsComposite = isCompositeNode,
        IsExpanded = isCompositeNode,
        IsDeleted = false
      };

      context.NavigationEntities.Add(entity);
      await context.SaveChangesAsync();
      return newNode;
    }

    return null;
  }
#endif
}
