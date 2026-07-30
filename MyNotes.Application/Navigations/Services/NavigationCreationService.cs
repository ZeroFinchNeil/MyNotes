using MyNotes.Application.Contracts.Database.Core;
using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Application.Contracts.Navigations.Persistence;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Querying.Models;
using MyNotes.Application.Contracts.Settings;
using MyNotes.Application.Navigations.Commands;
using MyNotes.Application.Settings.Services;
using MyNotes.Common.Exceptions;
using MyNotes.Domain.Navigations;

namespace MyNotes.Application.Navigations.Services;

internal sealed partial class NavigationCreationService
{
  private readonly INavigationRepository NavigationRepository;
  private readonly IAppDbTransactionFactory AppDbTransactionFactory;
  private readonly NavigationFactory NavigationFactory;
  private readonly AppSettingsService AppSettingsService;

  public NavigationCreationService(INavigationRepository navigationRepository, IAppDbTransactionFactory appDbTransactionFactory, NavigationFactory navigationFactory, AppSettingsService appSettingsService)
  {
    NavigationRepository = navigationRepository;
    AppDbTransactionFactory = appDbTransactionFactory;
    NavigationFactory = navigationFactory;
    AppSettingsService = appSettingsService;
  }

  public async Task<NavigationDto?> AddNavigationAsync(CreateNavigationAppCommand appCommand, CancellationToken cancellationToken = default)
  {
    NavigationId insertTargetId = appCommand.InsertTargetId;
    NavigationInsertPosition insertPosition = appCommand.InsertPosition;

    // Insert Target Navigation이 DB에 존재하는지 확인 후 Id와 Parent, IsComposite 속성 가져옴
    NavigationProjectionFields getFields = NavigationProjectionFields.ParentId | NavigationProjectionFields.IsComposite;
    NavigationProjectionDto insertTargetDto = insertTargetId == NavigationId.UserRoot
      ? new()
      {
        Id = NavigationId.UserRoot,
        ParentId = NavigationId.Empty,
        IsComposite = true
      }
      : await NavigationRepository.GetNavigationFieldValuesAsync(insertTargetId, getFields, cancellationToken);
    if (insertTargetDto.IsEmpty)
    {
      return null;
    }

    // Application과 Infra DB의 Target Navigation 정보 일치 확인 후 새 Navigation 추가
    if (insertTargetDto.ParentId.TryGet(out var targetParentId) && insertTargetDto.IsComposite.TryGet(out var isTargetComposite))
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
      Navigation navigation = NavigationFactory.Create(newNavigationId, parentId, appCommand.IsComposite, appCommand.Icon, appCommand.Title, AppDefaultSettings.IsNavigationDeleted);

      await using var appDbTransaction = await AppDbTransactionFactory.CreateAsync(cancellationToken);

      try
      {
        NavigationViewStateDto viewStateDto = appCommand.IsComposite
          ? new CompositeNavigationViewStateDto()
          {
            IsExpanded = true,
            Id = newNavigationId
          }
          : new LeafNavigationViewStateDto()
          {
            NoteSortKey = AppSettingsService.Load<NoteSortKey, int>(NoteSortKeySettingsCodec.Decode, NavigationSettingsDescriptors.NoteSortKey),
            NoteSortDirection = AppSettingsService.Load<SortDirection, int>(SortDirectionSettingsCodec.Decode, NavigationSettingsDescriptors.NoteSortDirection),
            PreviewLayoutType =  AppSettingsService.Load<PreviewLayoutType, int>(PreviewLayoutTypeSettingsCodec.Decode, NavigationSettingsDescriptors.PreviewLayoutType),
            PreviewTileSize =  AppSettingsService.Load<PreviewTileSize, int>(PreviewTileSizeSettingsCodec.Decode, NavigationSettingsDescriptors.PreviewTileSize),
            PreviewTileRatio =  AppSettingsService.Load<PreviewTileRatio, int>(PreviewTileRatioSettingsCodec.Decode, NavigationSettingsDescriptors.PreviewTileRatio),
            Id = newNavigationId
          };

        NavigationDto navigationDto = NavigationMappers.ToDto(navigation, viewStateDto);

        await NavigationRepository.AddNavigationAsync(navigationDto, insertTargetId, insertPosition, appDbTransaction, cancellationToken);
        await appDbTransaction.CompleteAsync(true, cancellationToken);

        return navigationDto;
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
