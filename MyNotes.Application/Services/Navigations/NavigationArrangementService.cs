using MyNotes.Application.Contracts.Database.Core;
using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Application.Dtos.Navigations.Arrangement;
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

  public async Task<MoveNavigationAppResponseDto> MoveNavigationAsync(MoveNavigationAppRequestDto moveAppRequestDto, CancellationToken cancellationToken = default)
  {
    var sourceId = moveAppRequestDto.SourceNavigation;
    var targetId = moveAppRequestDto.TargetNavigation;

    if (sourceId == targetId || await NavigationRepository.IsDescendantOfAsync(targetId, sourceId, cancellationToken))
    {
      return new()
      {
        ResultKind = MoveNavigationResultKind.Rejected,
        UpdatedNavigations = null
      };
    }

    await using var appDbTransaction = await AppDbTransactionFactory.CreateAsync(cancellationToken);
    try
    {
      var dbResponseDtos = await NavigationRepository.MoveNavigationAsync(NavigationMappers.ToDbDto(moveAppRequestDto), appDbTransaction, cancellationToken);

      var updatedNavigations = dbResponseDtos
        .Where(dto => dto.Id is not null)
        .Select(dto => dto.Id!.Value)
        .ToList();
      var updatedNavigationSet = updatedNavigations.ToHashSet();
      var expectedNavigationSet = moveAppRequestDto.ExpectedTargetSiblings.ToHashSet();

      MoveNavigationResultKind resultKind = MoveNavigationResultKind.Rejected;
      string? failureMessage = null;

      if (updatedNavigationSet.SetEquals(expectedNavigationSet))
      {
        if (updatedNavigations.SequenceEqual(moveAppRequestDto.ExpectedTargetSiblings))
        {
          resultKind = MoveNavigationResultKind.MovedAsRequested;
        }
        else
        {
          resultKind = MoveNavigationResultKind.MovedWithOrderReconciliation;
          failureMessage = "Navigation 이동은 완료되었지만 최종 순서가 요청 순서와 달라져 화면 순서를 다시 동기화해야 합니다."; //todo: 순서 불일치 상황에 맞게 실패 메시지 지정, Infra DB 업데이트 기준으로 Presentation에서 변경 순서 반영
        }
        await appDbTransaction.CompleteAsync(true, cancellationToken);
      }
      else
      {
        updatedNavigations = null;
        await appDbTransaction.RollbackAsync(CancellationToken.None);
        failureMessage = "이동 대상 목록이 변경되어 Navigation 이동을 완료할 수 없습니다."; //todo: 상황에 맞게 실패 메시지 지정
      }
      return new MoveNavigationAppResponseDto()
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
        await appDbTransaction.RollbackAsync(CancellationToken.None);
      }

      throw;
    }
  }
}