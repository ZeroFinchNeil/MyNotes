using MyNotes.Application.Contracts.Database.Dtos.Navigations;
using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Application.Dtos.Navigations;
using MyNotes.Application.Mappers;
using MyNotes.Common.Structures;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Services.Navigations;

internal sealed partial class NavigationArrangementService
{
  private readonly INavigationRepository NavigationRepository;

  public NavigationArrangementService(INavigationRepository navigationRepository)
  {
    NavigationRepository = navigationRepository;
  }

  public async Task<MoveUserNavigationAppResponseDto> MoveUserNavigationAsync(MoveUserNavigationAppRequestDto moveUserNavigationAppRequestDto)
  {
    var dbResponseDtos = await NavigationRepository.MoveUserNavigationAsync(UserNavigationMappers.ToDbDto(moveUserNavigationAppRequestDto));

    var infraSet = dbResponseDtos.Where(dto => dto.Id is not null).Select(dto => dto.Id!.Value).ToHashSet();
    var presentationSet = moveUserNavigationAppRequestDto.ExpectedTargetSiblings.ToHashSet();

    bool isMoveAllowed = true;

    string? failureMessage = null;

    if(!infraSet.SetEquals(presentationSet))
    {
      isMoveAllowed = false;
      failureMessage = ""; //todo: 상황에 맞게 실패 메시지 지정
    }
    else if(!infraSet.SequenceEqual(presentationSet))
    {
      failureMessage = ""; //todo: 순서 불일치 상황에 맞게 실패 메시지 지정, infraSet을 기준으로 Presentation에서 순서 반영
    }

    return new MoveUserNavigationAppResponseDto()
    {
      IsMoveAllowed = isMoveAllowed,
      UpdatedNavigations = [.. infraSet],
      FailureMessage = failureMessage
    };
  }
}