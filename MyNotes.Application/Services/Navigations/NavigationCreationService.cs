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
  private readonly UserNavigationFactory UserNavigationFactory;

  public NavigationCreationService(INavigationRepository navigationRepository, IAppDbTransactionFactory appDbTransactionFactory, UserNavigationFactory userNavigationFactory)
  {
    NavigationRepository = navigationRepository;
    AppDbTransactionFactory = appDbTransactionFactory;
    UserNavigationFactory = userNavigationFactory;
  }

  public async Task<UserNavigationBundleAppResponseDto?> AddUserNavigationAsync(CreateUserNavigationAppRequestDto createUserNavigationAppRequestDto, CancellationToken cancellationToken = default)
  {
    NavigationId insertTargetId = createUserNavigationAppRequestDto.InsertTargetId;
    NavigationInsertPosition insertPosition = createUserNavigationAppRequestDto.NavigationInsertPosition;

    // Insert Target Navigation이 DB에 존재하는지 확인 후 Id와 Parent, IsComposite 속성 가져옴
    UserNavigationGetFields userNavigationGetFields = UserNavigationGetFields.Id | UserNavigationGetFields.Parent | UserNavigationGetFields.IsComposite;
    GetUserNavigationFieldValuesDbRequestDto getUserNavigationFieldsDbRequestDto = new()
    {
      UserNavigationGetFields = userNavigationGetFields,
      Id = insertTargetId
    };

    GetUserNavigationFieldValuesDbResponseDto getUserNavigationFieldsDbResponseDto = insertTargetId == NavigationId.UserRoot
      ? new()
      {
        UserNavigationGetFields = userNavigationGetFields,
        Id = insertTargetId,
        Parent = NavigationId.UserRoot,
        IsComposite = true
      }
      : await NavigationRepository.GetUserNavigationFieldValuesAsync(getUserNavigationFieldsDbRequestDto, cancellationToken);

    // Application과 Infra DB의 Target Navigation 정보 일치 확인 후 새 Navigation 추가
    if (getUserNavigationFieldsDbResponseDto.UserNavigationGetFields.Equals(getUserNavigationFieldsDbRequestDto.UserNavigationGetFields)
      && getUserNavigationFieldsDbResponseDto.Id == insertTargetId
      && getUserNavigationFieldsDbResponseDto.Parent is NavigationId targetParentId
      && getUserNavigationFieldsDbResponseDto.IsComposite is bool isTargetComposite)
    {
      if ((insertPosition is NavigationInsertPosition.FirstChild or NavigationInsertPosition.LastChild) && !isTargetComposite)
      {
        throw new InvalidStateException("Composite이 아닌 Navigation에 자식 요소로 추가할 수 없습니다.");
      }

      // DB에 있는 Navigation들과 일치하지 않는 Unique Id 생성 -> 새로운 Navigation의 Id로 사용
      NavigationId newNavigationId = await NavigationRepository.GenerateUniqueUserNavigationIdAsync(cancellationToken);

      var parentId = insertPosition switch
      {
        NavigationInsertPosition.Before or NavigationInsertPosition.After => targetParentId,
        NavigationInsertPosition.FirstChild or NavigationInsertPosition.LastChild => insertTargetId,
        _ => throw new InvalidOperationException()
      };

      // UserNavigation Domain Entity로 변환하여 도메인 속성 유효성 검사
      UserNavigation userNavigation = UserNavigationFactory.Create(newNavigationId, parentId, createUserNavigationAppRequestDto.IsComposite, (int)createUserNavigationAppRequestDto.Icon, createUserNavigationAppRequestDto.Title, false);

      await using var appDbTransaction = await AppDbTransactionFactory.CreateAsync(cancellationToken);

      try
      {
        UserNavigationBundleDbResponseDto bundleDbResponseDto = await NavigationRepository.AddUserNavigationAsync(UserNavigationMappers.ToCreateDbDto(userNavigation, insertTargetId, insertPosition), appDbTransaction, cancellationToken);

        await appDbTransaction.CompleteAsync(true, cancellationToken);

        UserNavigationDbResponseDto dbResponseDto = bundleDbResponseDto.UserNavigationDto;
        UserNavigationViewStateDbResponseDto viewStateDbResponseDto = bundleDbResponseDto.ViewStateDto;

        return UserNavigationMappers.BundleAppDto(UserNavigationMappers.ToAppDto(dbResponseDto), UserNavigationMappers.ToAppDto(viewStateDbResponseDto));
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
