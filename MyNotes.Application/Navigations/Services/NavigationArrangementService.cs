using MyNotes.Application.Commands.Navigations;
using MyNotes.Application.Contracts.Database.Core;
using MyNotes.Application.Contracts.Navigations.Persistence;
using MyNotes.Application.Navigations.Results;

namespace MyNotes.Application.Navigations.Services;

internal sealed partial class NavigationArrangementService
{
  private readonly INavigationRepository NavigationRepository;
  private readonly IAppDbTransactionFactory AppDbTransactionFactory;

  public NavigationArrangementService(INavigationRepository navigationRepository, IAppDbTransactionFactory appDbTransactionFactory)
  {
    NavigationRepository = navigationRepository;
    AppDbTransactionFactory = appDbTransactionFactory;
  }

  public async Task<MoveNavigationResult> MoveNavigationAsync(MoveNavigationAppCommand appCommand, CancellationToken cancellationToken = default)
  {
    var sourceId = appCommand.SourceNavigationId;
    var targetId = appCommand.TargetNavigationId;

    if (sourceId == targetId || await NavigationRepository.IsDescendantOfAsync(targetId, sourceId, cancellationToken))
    {
      return new()
      {
        Kind = MoveNavigationResultKind.Rejected,
        UpdatedNavigations = null
      };
    }

    await using var appDbTransaction = await AppDbTransactionFactory.CreateAsync(cancellationToken);
    try
    {
      var projectionDtos = await NavigationRepository.MoveNavigationAsync(sourceId, targetId, appCommand.InsertPosition, appDbTransaction, cancellationToken);

      var updatedNavigations = projectionDtos.Select(dto => dto.Id).ToList();
      var updatedNavigationSet = updatedNavigations.ToHashSet();
      var expectedNavigationSet = appCommand.ExpectedTargetSiblings.ToHashSet();

      MoveNavigationResultKind resultKind = MoveNavigationResultKind.Rejected;
      string? failureMessage = null;

      if (updatedNavigationSet.SetEquals(expectedNavigationSet))
      {
        if (updatedNavigations.SequenceEqual(appCommand.ExpectedTargetSiblings))
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

      return new()
      {
        Kind = resultKind,
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