using MyNotes.Application.Contracts.Database.Core;
using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Application.Dtos.Navigations;
using MyNotes.Application.Enums.Navigations;
using MyNotes.Application.Mappers;

namespace MyNotes.Application.Services.Navigations;

internal sealed partial class NavigationArrangementService
{
  private readonly INavigationRepository NavigationRepository;
  private readonly IAppDbTransactionFactory AppDbTransactionFactory;

  public NavigationArrangementService(INavigationRepository navigationRepository, IAppDbTransactionFactory appDbTransactionFactory)
  {
    NavigationRepository = navigationRepository;
    AppDbTransactionFactory = appDbTransactionFactory;
  }

  public async Task<MoveUserNavigationAppResponseDto> MoveUserNavigationAsync(MoveUserNavigationAppRequestDto moveUserNavigationAppRequestDto)
  {
    await using var appDbTransaction = await AppDbTransactionFactory.CreateAsync();
    try
    {
      var dbResponseDtos = await NavigationRepository.MoveUserNavigationAsync(UserNavigationMappers.ToDbDto(moveUserNavigationAppRequestDto), appDbTransaction);

      var updatedNavigations = dbResponseDtos
        .Where(dto => dto.Id is not null)
        .Select(dto => dto.Id!.Value)
        .ToList();
      var updatedNavigationSet = updatedNavigations.ToHashSet();
      var expectedNavigationSet = moveUserNavigationAppRequestDto.ExpectedTargetSiblings.ToHashSet();

      MoveUserNavigationResultKind resultKind = MoveUserNavigationResultKind.Rejected;
      string? failureMessage = null;

      if (updatedNavigationSet.SetEquals(expectedNavigationSet))
      {
        if (updatedNavigations.SequenceEqual(moveUserNavigationAppRequestDto.ExpectedTargetSiblings))
        {
          resultKind = MoveUserNavigationResultKind.MovedAsRequested;
        }
        else
        {
          resultKind = MoveUserNavigationResultKind.MovedWithOrderReconciliation;
          failureMessage = "Navigation 이동은 완료되었지만 최종 순서가 요청 순서와 달라져 화면 순서를 다시 동기화해야 합니다."; //todo: 순서 불일치 상황에 맞게 실패 메시지 지정, Infra DB 업데이트 기준으로 Presentation에서 변경 순서 반영
        }
        await appDbTransaction.CompleteAsync();
      }
      else
      {
        updatedNavigations = null;
        await appDbTransaction.RollbackAsync();
        failureMessage = "이동 대상 목록이 변경되어 Navigation 이동을 완료할 수 없습니다."; //todo: 상황에 맞게 실패 메시지 지정
      }
      return new MoveUserNavigationAppResponseDto()
      {
        ResultKind = resultKind,
        UpdatedNavigations = updatedNavigations,
        FailureMessage = failureMessage
      };
    }
    catch
    {
      if (!appDbTransaction.IsCompleted && !appDbTransaction.IsRolledBack)
      {
        await appDbTransaction.RollbackAsync();
      }

      throw;
    }
  }
}