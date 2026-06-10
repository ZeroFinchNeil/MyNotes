using MyNotes.Application.Contracts.Database.Dtos.Navigations.Common;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Creation;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Retrieval;
using MyNotes.Application.Contracts.Database.Enums.Navigations;
using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Application.Dtos.Navigations.Common;
using MyNotes.Application.Dtos.Navigations.Creation;
using MyNotes.Application.Mappers;
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

  public async Task<UserNavigationBundleAppResponseDto?> AddUserNavigationAsync(CreateUserNavigationAppRequestDto createUserNavigationAppRequestDto)
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
    GetUserNavigationFieldValuesDbResponseDto getUserNavigationFieldsDbResponseDto = await NavigationRepository.GetUserNavigationFieldValuesAsync(getUserNavigationFieldsDbRequestDto);

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

      UserNavigationBundleDbResponseDto dbAggregateResponseDto = await NavigationRepository.AddUserNavigationAsync(new CreateUserNavigationDbRequestDto()
      {
        Id = newNavigationId,
        InsertTargetId = insertTargetId,
        NavigationInsertPosition = insertPosition,
        IsComposite = userNavigation.IsComposite,
        Icon = userNavigation.Icon,
        Title = userNavigation.Title,
      });

      UserNavigationDbResponseDto dbResponseDto = dbAggregateResponseDto.UserNavigationDto;
      UserNavigationViewStateDbResponseDto viewStateDbResponseDto = dbAggregateResponseDto.ViewStateDto;

      return new(
        userNavigationDto: UserNavigationMappers.ToAppDto(dbResponseDto),
        viewStateDto: UserNavigationMappers.ToAppDto(viewStateDbResponseDto));
    }

    return null;
  }
}
