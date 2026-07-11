using MyNotes.Application.Contracts.Database.Core;
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
  private readonly IAppDbTransactionFactory AppDbTransactionFactory;
  private readonly NavigationFactory NavigationFactory;

  public NavigationCreationService(INavigationRepository navigationRepository, IAppDbTransactionFactory appDbTransactionFactory, NavigationFactory navigationFactory)
  {
    NavigationRepository = navigationRepository;
    AppDbTransactionFactory = appDbTransactionFactory;
    NavigationFactory = navigationFactory;
  }

  public async Task<NavigationBundleAppResponseDto?> AddNavigationAsync(CreateNavigationAppRequestDto createAppRequestDto, CancellationToken cancellationToken = default)
  {
    NavigationId insertTargetId = createAppRequestDto.InsertTargetId;
    NavigationInsertPosition insertPosition = createAppRequestDto.InsertPosition;

    // Insert Target Navigation이 DB에 존재하는지 확인 후 Id와 Parent, IsComposite 속성 가져옴
    NavigationGetFields getFields = NavigationGetFields.Id | NavigationGetFields.Parent | NavigationGetFields.IsComposite;
    GetNavigationFieldValuesDbRequestDto getFieldValuesDbRequestDto = new()
    {
      GetFields = getFields,
      Id = insertTargetId
    };

    GetNavigationFieldValuesDbResponseDto getFieldValuesDbResponseDto = insertTargetId == NavigationId.UserRoot
      ? new()
      {
        GetFields = getFields,
        Id = insertTargetId,
        Parent = NavigationId.UserRoot,
        IsComposite = true
      }
      : await NavigationRepository.GetNavigationFieldValuesAsync(getFieldValuesDbRequestDto, cancellationToken);

    // Application과 Infra DB의 Target Navigation 정보 일치 확인 후 새 Navigation 추가
    if (getFieldValuesDbResponseDto.GetFields.Equals(getFieldValuesDbRequestDto.GetFields)
      && getFieldValuesDbResponseDto.Id == insertTargetId
      && getFieldValuesDbResponseDto.Parent is NavigationId targetParentId
      && getFieldValuesDbResponseDto.IsComposite is bool isTargetComposite)
    {
      if ((insertPosition is NavigationInsertPosition.FirstChild or NavigationInsertPosition.LastChild) && !isTargetComposite)
      {
        throw new InvalidStateException("Composite이 아닌 Navigation에 자식 요소로 추가할 수 없습니다.");
      }

      // DB에 있는 Navigation들과 일치하지 않는 Unique Id 생성 -> 새로운 Navigation의 Id로 사용
      NavigationId newNavigationId = await NavigationRepository.GenerateUniqueNavigationIdAsync(cancellationToken);

      var parentId = insertPosition switch
      {
        NavigationInsertPosition.Before or NavigationInsertPosition.After => targetParentId,
        NavigationInsertPosition.FirstChild or NavigationInsertPosition.LastChild => insertTargetId,
        _ => throw new InvalidOperationException()
      };

      // Navigation Domain Entity로 변환하여 도메인 속성 유효성 검사
      Navigation navigation = NavigationFactory.Create(newNavigationId, parentId, createAppRequestDto.IsComposite, (int)createAppRequestDto.Icon, createAppRequestDto.Title, false);

      await using var appDbTransaction = await AppDbTransactionFactory.CreateAsync(cancellationToken);

      try
      {
        NavigationBundleDbResponseDto bundleDbResponseDto = await NavigationRepository.AddNavigationAsync(NavigationMappers.ToCreateDbDto(navigation, insertTargetId, insertPosition), appDbTransaction, cancellationToken);

        await appDbTransaction.CompleteAsync(true, cancellationToken);

        NavigationDbResponseDto dbResponseDto = bundleDbResponseDto.NavigationDto;
        NavigationViewStateDbResponseDto viewStateDbResponseDto = bundleDbResponseDto.ViewStateDto;

        return NavigationMappers.BundleAppDto(NavigationMappers.ToAppDto(dbResponseDto), NavigationMappers.ToAppDto(viewStateDbResponseDto));
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

    return null;
  }
}
