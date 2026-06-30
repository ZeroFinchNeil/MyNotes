using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using MyNotes.Application.Contracts.Database.Core;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Arrangement;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Common;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Creation;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Modification;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Retrieval;
using MyNotes.Application.Contracts.Database.Enums.Navigations;
using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Domain.ValueObjects;
using MyNotes.Infrastructure.Database.Core;
using MyNotes.Infrastructure.Database.Entities.Navigations;
using MyNotes.Infrastructure.Mappers;

namespace MyNotes.Infrastructure.Database.Repositories.Navigations;

internal class NavigationRepository : INavigationRepository
{
  private readonly IDbContextFactory<AppDbContext> DbContextFactory;

  public NavigationRepository(IDbContextFactory<AppDbContext> dbContextFactory)
  {
    DbContextFactory = dbContextFactory;
  }

  // 추후 join을 이용한 방식으로 리팩터링: 예시 ->
  // public async Task<IReadOnlyList<UserNavigationBundleDbResponseDto>> GetAllUserNavigationsAsync(CancellationToken cancellationToken = default)
  //{
  //  await using AppDbContext context =
  //      await DbContextFactory.CreateDbContextAsync(cancellationToken);

  //  // Composite Navigation과 Composite ViewState를 공유 키 Id로 조인합니다.
  //  var compositePairs = await context.UserNavigationEntities
  //      .AsNoTracking()
  //      .Where(navigation => navigation.IsComposite)
  //      .Join(
  //          context.UserCompositeNavigationViewStateEntity.AsNoTracking(),
  //          navigation => navigation.Id,
  //          viewState => viewState.Id,
  //          (navigation, viewState) => new
  //          {
  //            Navigation = navigation,
  //            ViewState = viewState
  //          })
  //      .ToListAsync(cancellationToken);

  //  // Leaf Navigation과 Leaf ViewState를 공유 키 Id로 조인합니다.
  //  var leafPairs = await context.UserNavigationEntities
  //      .AsNoTracking()
  //      .Where(navigation => !navigation.IsComposite)
  //      .Join(
  //          context.UserLeafNavigationViewStateEntity.AsNoTracking(),
  //          navigation => navigation.Id,
  //          viewState => viewState.Id,
  //          (navigation, viewState) => new
  //          {
  //            Navigation = navigation,
  //            ViewState = viewState
  //          })
  //      .ToListAsync(cancellationToken);

  //  var compositeBundles = compositePairs.Select(pair =>
  //      new UserNavigationBundleDbResponseDto(
  //          UserNavigationMappers.ToDto(pair.Navigation),
  //          UserNavigationMappers.ToDto(pair.ViewState)));

  //  var leafBundles = leafPairs.Select(pair =>
  //      new UserNavigationBundleDbResponseDto(
  //          UserNavigationMappers.ToDto(pair.Navigation),
  //          UserNavigationMappers.ToDto(pair.ViewState)));

  //  return compositeBundles
  //      .Concat(leafBundles)
  //      .ToArray();
  //}
  public async Task<IReadOnlyList<UserNavigationBundleDbResponseDto>> GetAllUserNavigationsAsync(CancellationToken cancellationToken = default)
  {
    await using AppDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken);
    var userNavigationEntities = await context.UserNavigationEntities.AsNoTracking().ToListAsync(cancellationToken);
    var navigationIds = userNavigationEntities.Select(e => e.Id).ToHashSet();
    var compositeViewStateEntitiesById = await context.UserCompositeNavigationViewStateEntity
      .AsNoTracking()
      .Where(e => navigationIds.Contains(e.Id))
      .ToDictionaryAsync(e => e.Id, cancellationToken);
    var leafViewStateEntitiesById = await context.UserLeafNavigationViewStateEntity
      .AsNoTracking()
      .Where(e => navigationIds.Contains(e.Id))
      .ToDictionaryAsync(e => e.Id, cancellationToken);

    return [.. userNavigationEntities.Select(userNavigationEntity =>
    {
      UserNavigationDbResponseDto userNavigationDbResponseDto = UserNavigationMappers.ToDto(userNavigationEntity);
      UserNavigationViewStateDbResponseDto? userNavigationViewStateDbResponseDto = userNavigationEntity.IsComposite
        ? compositeViewStateEntitiesById.TryGetValue(userNavigationEntity.Id, out var compositeViewStateEntity)
          ? UserNavigationMappers.ToDto(compositeViewStateEntity)
          : null
        : leafViewStateEntitiesById.TryGetValue(userNavigationEntity.Id, out var leafViewStateEntity)
          ? UserNavigationMappers.ToDto(leafViewStateEntity)
          : null;
      return userNavigationViewStateDbResponseDto is not null
        ? new UserNavigationBundleDbResponseDto(userNavigationDbResponseDto, userNavigationViewStateDbResponseDto)
        : null;
    }).OfType<UserNavigationBundleDbResponseDto>()];
  }

  public async Task<UserNavigationBundleDbResponseDto?> GetUserNavigationByIdAsync(NavigationId id, CancellationToken cancellationToken = default)
  {
    await using AppDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken);

    if (await context.UserNavigationEntities.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id.Value, cancellationToken) is UserNavigationEntity userNavigationEntity)
    {
      UserNavigationDbResponseDto userNavigationDbResponseDto = UserNavigationMappers.ToDto(userNavigationEntity);
      UserNavigationViewStateDbResponseDto? userNavigationViewStateDbResponseDto = userNavigationEntity.IsComposite
        ? await context.UserCompositeNavigationViewStateEntity.AsNoTracking().FirstOrDefaultAsync(e => e.Id == userNavigationEntity.Id, cancellationToken) is UserCompositeNavigationViewStateEntity compositeViewStateEntity
          ? UserNavigationMappers.ToDto(compositeViewStateEntity)
          : null
        : await context.UserLeafNavigationViewStateEntity.AsNoTracking().FirstOrDefaultAsync(e => e.Id == userNavigationEntity.Id, cancellationToken) is UserLeafNavigationViewStateEntity leafViewStateEntity
          ? UserNavigationMappers.ToDto(leafViewStateEntity)
          : null;
      return userNavigationViewStateDbResponseDto is not null
        ? new UserNavigationBundleDbResponseDto(userNavigationDbResponseDto, userNavigationViewStateDbResponseDto)
        : null;
    }

    return null;
  }

  public async Task<NavigationId> GenerateUniqueUserNavigationIdAsync(CancellationToken cancellationToken = default)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);

    NavigationId navigationId;
    do
    {
      navigationId = NavigationId.NewId();
    } while (await context.UserNavigationEntities.AnyAsync(e => e.Id == navigationId.Value, cancellationToken));

    return navigationId;
  }

  public Task<GetUserNavigationFieldValuesDbResponseDto> GetUserNavigationFieldValuesAsync(GetUserNavigationFieldValuesDbRequestDto getUserNavigationFieldsDbRequestDto, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }

  public Task<UserNavigationBundleDbResponseDto> AddUserNavigationAsync(CreateUserNavigationDbRequestDto createUserNavigationDbRequestDto, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }

  public Task<UpdateUserNavigationDbResponseDto> UpdateUserNavigationAsync(UpdateUserNavigationDbRequestDto updateUserNavigationDbRequestDto, bool updateIfChanged = true, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }

  public Task<bool> DeleteUserNavigationAsync(DeleteUserNavigationDbRequestDto deleteUserNavigationDbRequestDto, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }

  private const int TemporarySourcePosition = int.MinValue;

  public async Task<IReadOnlyList<GetUserNavigationFieldValuesDbResponseDto>> MoveUserNavigationAsync(MoveUserNavigationDbRequestDto moveUserNavigationDbRequestDto, IAppDbTransactionContext? appDbTransactionContext = null, CancellationToken cancellationToken = default)
  {
    // Source Navigation을 포함하여 Position에 영향받는 모든 Navigation들의 Id, Parent, Position을 담아서 반환(이 때 반환되는 모든 Navigation의 Parent 속성 값은 모두 일치해야 함).
    List<GetUserNavigationFieldValuesDbResponseDto> resultDtos = new();
    UserNavigationGetFields userNavigationGetField = UserNavigationGetFields.Id | UserNavigationGetFields.Parent | UserNavigationGetFields.Position;

    Guid sourceId = moveUserNavigationDbRequestDto.SourceNavigation.Value;
    Guid targetId = moveUserNavigationDbRequestDto.TargetNavigation.Value;
    NavigationInsertPosition insertPosition = moveUserNavigationDbRequestDto.NavigationInsertPosition;

    if (sourceId == targetId)
    {
      throw new ArgumentException($"Source Navigation과 Target Navigation은 동일할 수 없습니다. SourceId={sourceId}, TargetId={targetId}", nameof(moveUserNavigationDbRequestDto));
    }

    AppDbContext context = appDbTransactionContext?.DbContext switch
    {
      AppDbContext appDbContext => appDbContext,
      null when appDbTransactionContext is null => await DbContextFactory.CreateDbContextAsync(),
      null => throw new InvalidOperationException("DB 트랜잭션 컨텍스트가 초기화되지 않았습니다."),
      DbContext dbContext => throw new InvalidOperationException(
          $"지원하지 않는 DbContext 타입입니다. Expected: {typeof(AppDbContext).FullName}, Actual: {dbContext.GetType().FullName}")
    };

    await using IDbContextTransaction? localTransaction = appDbTransactionContext is null
      ? await context.Database.BeginTransactionAsync(cancellationToken)
      : null;

    bool ownsTransaction = localTransaction is not null;

    try
    {
      var sourceEntity = await context.UserNavigationEntities.AsNoTracking().FirstOrDefaultAsync(entity => entity.Id == sourceId, cancellationToken)
        ?? throw new InvalidOperationException($"Source Navigation을 찾을 수 없습니다. Id={sourceId}");

      var targetEntity = await context.UserNavigationEntities.AsNoTracking().FirstOrDefaultAsync(entity => entity.Id == targetId, cancellationToken)
        ?? throw new InvalidOperationException($"Target Navigation을 찾을 수 없습니다. Id={targetId}");

      Guid oldSourceParent = sourceEntity.Parent;
      Guid newSourceParent = insertPosition switch
      {
        NavigationInsertPosition.Before or NavigationInsertPosition.After => targetEntity.Parent,
        NavigationInsertPosition.FirstChild or NavigationInsertPosition.LastChild => targetEntity.Id,
        _ => throw new NotSupportedException($"지원하지 않는 Navigation 삽입 위치입니다. InsertPosition={insertPosition}")
      };
      int oldSourcePosition = sourceEntity.Position;

      // 1. siblingEntities는 sourceEntity 이동 완료 후 sourceEntity와 Parent가 같아질 형제 Entity 목록이며, Position 오름차순으로 조회
      // 2. ToListAsync로 생성한 조회 결과 목록이므로 컬렉션 항목의 추가/제거/삽입은 DB 변경과 무관함
      // 3. AsNoTracking으로 조회한 Entity들이므로 각 Entity의 속성 변경도 자동으로 DB에 반영되지 않음
      // 4. DB 반영은 이후 명시적인 업데이트 로직에서 반영해야 함(ExecuteUpdateAsync 사용)
      List<UserNavigationEntity> siblingEntities = await context.UserNavigationEntities
        .AsNoTracking()
        .Where(e => e.Parent == newSourceParent)
        .OrderBy(e => e.Position)
        .ToListAsync(cancellationToken);

      siblingEntities.RemoveAll(entity => entity.Id == sourceEntity.Id);

      int insertedIndex = insertPosition switch
      {
        NavigationInsertPosition.Before => siblingEntities.IndexOf(targetEntity),
        NavigationInsertPosition.After => siblingEntities.IndexOf(targetEntity) + 1,
        NavigationInsertPosition.FirstChild => 0,
        NavigationInsertPosition.LastChild => siblingEntities.Count,
        _ => throw new NotSupportedException($"지원하지 않는 Navigation 삽입 위치입니다. InsertPosition={insertPosition}")
      };

      if (insertedIndex < 0)
      {
        throw new InvalidOperationException($"Target Navigation이 이동 후 형제 목록에 없습니다. TargetId={targetId}, NewSourceParent={newSourceParent}, InsertPosition={insertPosition}");
      }

      // sourceEntity의 Parent를 변경하고 임시 Position 설정 후 siblingsEntities에 삽입
      if (!await EnsureTemporarySourceSlotIsAvailableAsync(context, newSourceParent))
      {
        throw new InvalidOperationException($"source Navigation 임시 Position이 이미 사용 중입니다. Parent={newSourceParent}, Position={TemporarySourcePosition}");
      }

      sourceEntity.Parent = newSourceParent;
      sourceEntity.Position = TemporarySourcePosition;
      int sourceTemporaryMoveRows = await context.UserNavigationEntities
        .Where(e => e.Id == sourceEntity.Id
                    && e.Parent == oldSourceParent
                    && e.Position == oldSourcePosition)
        .ExecuteUpdateAsync(setters => setters
          .SetProperty(e => e.Parent, sourceEntity.Parent)
          .SetProperty(e => e.Position, sourceEntity.Position)
        , cancellationToken);

      if (sourceTemporaryMoveRows != 1)
      {
        throw new InvalidOperationException($"source Navigation 임시 Position 반영에 실패했습니다. Id={sourceEntity.Id}, AffectedRows={sourceTemporaryMoveRows}");
      }

      siblingEntities.Insert(insertedIndex, sourceEntity);

      // 위치 변경을 완료한 후에 영향받는 Navigation들만 Position을 수정할 수 있도록 repositionEntities 컬렉션 생성 후 위치 재조정
      Dictionary<Guid, int> originalSiblingPositionById = siblingEntities.ToDictionary(e => e.Id, e => e.Position);
      var affectedEntities = UserNavigationRepositioner.RepositionFromInsertedNavigation(siblingEntities, insertedIndex, out RepositionRangeKind siblingsRepositionRangeKind);
      if (affectedEntities.Count == 0)
      {
        throw new InvalidOperationException($"Navigation 재배치 결과 변경된 Position이 없습니다. SourceId={sourceId}, InsertedIndex={insertedIndex}, RepositionRangeKind={siblingsRepositionRangeKind}");
      }
      var affectedOrderedEntities = affectedEntities.OrderBy(e => e.Position).ToList();

      switch (siblingsRepositionRangeKind)
      {
        case RepositionRangeKind.SourceOnly:
          if (affectedOrderedEntities.Count != 1 || affectedOrderedEntities[0].Id != sourceId)
          {
            throw new InvalidOperationException("SourceOnly 재배치에서는 source Navigation만 Position이 변경되어야 합니다.");
          }
          break;
        case RepositionRangeKind.ExpandedToLeft:
          if (affectedOrderedEntities[^1].Id != sourceId)
          {
            throw new InvalidOperationException($"왼쪽 확장 재배치 결과에서 source가 마지막 Position이어야 합니다. SourceId={sourceId}, LastAffectedId={affectedOrderedEntities[^1].Id}");
          }
          for (int index = 0; index < affectedOrderedEntities.Count - 1; index++)
          {
            await UpdateSiblingPositionAsync(affectedOrderedEntities[index]);
          }
          break;
        case RepositionRangeKind.ExpandedToRight:
          if (affectedOrderedEntities[0].Id != sourceId)
          {
            throw new InvalidOperationException($"오른쪽 확장 재배치 결과에서 source가 첫 번째 Position이어야 합니다. SourceId={sourceId}, FirstAffectedId={affectedOrderedEntities[0].Id}");
          }
          for (int index = affectedOrderedEntities.Count - 1; index > 0; index--)
          {
            await UpdateSiblingPositionAsync(affectedOrderedEntities[index]);
          }
          break;
        default:
          throw new NotSupportedException(
            $"지원하지 않는 RepositionRangeKind입니다. Kind={siblingsRepositionRangeKind}");
      }

      // sourceEntity의 정상 Position 반영
      var sourceFinalMoveRows = await context.UserNavigationEntities
        .Where(e => e.Id == sourceEntity.Id
                    && e.Parent == newSourceParent
                    && e.Position == TemporarySourcePosition)
        .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.Position, sourceEntity.Position), cancellationToken);

      if (sourceFinalMoveRows != 1)
      {
        throw new InvalidOperationException($"source Navigation 최종 Position 반영에 실패했습니다. source가 임시 위치에 없거나 동시 변경되었을 수 있습니다. Id={sourceEntity.Id}, Parent={newSourceParent}, ExpectedPosition={TemporarySourcePosition}, NewPosition={sourceEntity.Position}, AffectedRows={sourceFinalMoveRows}");
      }

      foreach (var siblingEntity in siblingEntities)
      {
        resultDtos.Add(new GetUserNavigationFieldValuesDbResponseDto()
        {
          UserNavigationGetFields = userNavigationGetField,
          Id = NavigationId.Create(siblingEntity.Id),
          Parent = NavigationId.Create(siblingEntity.Parent),
          Position = siblingEntity.Position
        });
      }

      if (ownsTransaction)
      {
        await localTransaction!.CommitAsync(cancellationToken);
      }

      async Task UpdateSiblingPositionAsync(UserNavigationEntity siblingEntity)
      {
        if (!originalSiblingPositionById.TryGetValue(siblingEntity.Id, out var affectedEntityOriginalPosition))
        {
          throw new InvalidOperationException($"Navigation의 원래 Position을 찾을 수 없습니다. Id={siblingEntity.Id}, Parent={newSourceParent}");
        }
        int affectedRows = await context.UserNavigationEntities
          .Where(e => e.Id == siblingEntity.Id
                      && e.Parent == newSourceParent
                      && e.Position == affectedEntityOriginalPosition)
          .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.Position, siblingEntity.Position), cancellationToken);

        if (affectedRows != 1)
        {
          throw new InvalidOperationException($"Navigation Position 업데이트에 실패했습니다. 형제 Navigation의 Position이 동시 변경되었을 수 있습니다. Id={siblingEntity.Id}, Parent={newSourceParent}, ExpectedPosition={affectedEntityOriginalPosition}, NewPosition={siblingEntity.Position}, AffectedRows={affectedRows}");
        }
      }
    }
    catch
    {
      if (ownsTransaction)
      {
        await localTransaction!.RollbackAsync(cancellationToken);
      }
      throw;
    }
    finally
    {
      if (ownsTransaction)
      {
        await context.DisposeAsync();
      }
    }

    return resultDtos;
  }

  private static async Task<bool> EnsureTemporarySourceSlotIsAvailableAsync(AppDbContext context, Guid parentId) =>
    !await context.UserNavigationEntities.AsNoTracking().AnyAsync(e => e.Parent == parentId && e.Position == TemporarySourcePosition);

  private const int DefaultPositionStep = 1024;
  private const int MaxPositionStep = 4096;

  /// <summary>
  /// source 삽입 후 Position 재계산이 어떤 범위에서 발생했는지 나타냅니다.
  /// </summary>
  private enum RepositionRangeKind { SourceOnly, ExpandedToLeft, ExpandedToRight }

  private sealed class UserNavigationRepositioner
  {
    private readonly IList<UserNavigationEntity> _entities;
    private readonly List<UserNavigationEntity> _positionChangedEntities = new();

    private int Count => _entities.Count;

    private UserNavigationRepositioner(IList<UserNavigationEntity> entities)
    {
      ArgumentNullException.ThrowIfNull(entities);
      _entities = entities;
    }

    public static IReadOnlyList<UserNavigationEntity> RepositionFromInsertedNavigation(IList<UserNavigationEntity> entities, int insertedIndex, out RepositionRangeKind repositionRangeKind) => new UserNavigationRepositioner(entities)
      .RepositionFromInsertedNavigation(insertedIndex, out repositionRangeKind);

    private IReadOnlyList<UserNavigationEntity> RepositionFromInsertedNavigation(int insertedIndex, out RepositionRangeKind repositionRangeKind)
    {
      ArgumentOutOfRangeException.ThrowIfLessThan(insertedIndex, 0, nameof(insertedIndex));
      ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(insertedIndex, Count, nameof(insertedIndex));

      repositionRangeKind = RepositionRangeKind.SourceOnly;

      // 추가한 후 컬렉션 항목이 하나라면 추가한 항목의 Position은 0임
      if (Count == 1)
      {
        AddAffectedIfPositionChanged(_entities[insertedIndex], 0);
        return _positionChangedEntities;
      }

      // 항목이 맨 앞에 추가되었다면, 기존 첫 번째 항목의 Position보다 작은 Position을 계산하여 설정
      if (insertedIndex == 0)
      {
        AddAffectedIfPositionChanged(_entities[0], checked(_entities[1].Position - CalculatePositionStep(1, null, _entities[1].Position)));
        return _positionChangedEntities;
      }

      // 항목이 맨 뒤에 추가되었다면, 기존 맨 마지막 항목의 Position보다 큰 Position을 계산하여 설정
      if (insertedIndex == Count - 1)
      {
        AddAffectedIfPositionChanged(_entities[^1], checked(_entities[^2].Position + CalculatePositionStep(1, _entities[^2].Position, null)));
        return _positionChangedEntities;
      }

      // 후보 Position이 앞뒤와 겹치지 않는다면 그대로 확정
      int prevPos = _entities[insertedIndex - 1].Position;
      int nextPos = _entities[insertedIndex + 1].Position;
      long posDiff = (long)nextPos - prevPos;

      if (posDiff < 0)
      {
        throw new InvalidOperationException($"Position 순서가 올바르지 않습니다. prevPos={prevPos}, nextPos={nextPos}");
      }

      // Position 변경된 범위 인덱스 양끝 중에 insertedIndex가 있으면
      // 재조정 간격(step)을 계산하기 위해 insertedIndex의 왼쪽 혹은 오른쪽 항목의 Position을 알아야 함
      // insertedIndex <= 0 이나 insertedIndex >= Count - 1인 상황은 위에서 걸러짐
      int? leftBoundaryPos = _entities[insertedIndex - 1].Position;
      int? rightBoundaryPos = _entities[insertedIndex + 1].Position;

      // 바로 양옆 사이에 빈 Position이 없으면 주변부를 확장 탐색
      for (int indexGap = 1; ; indexGap++)
      {
        int leftIndex = insertedIndex - indexGap;
        int rightIndex = insertedIndex + indexGap;

        // 왼쪽 끝에 도달했을 경우
        // 왼쪽 boundary가 없으므로 [0, insertedIndex] 범위를 오른쪽 boundary 기준으로 다시 배치
        // source는 재배치 범위의 마지막 항목이 됨
        if (leftIndex < 0)
        {
          RepositionRange(0, insertedIndex, null, rightBoundaryPos);
          repositionRangeKind = RepositionRangeKind.ExpandedToLeft;
          return _positionChangedEntities;
        }

        // 오른쪽 끝에 도달했을 경우
        // 오른쪽 boundary가 없으므로 [insertedIndex, Count - 1] 범위를 왼쪽 boundary 기준으로 다시 배치
        // source는 재배치 범위의 첫 번째 항목이 됨
        if (rightIndex >= Count)
        {
          RepositionRange(insertedIndex, Count - 1, leftBoundaryPos, null);
          repositionRangeKind = RepositionRangeKind.ExpandedToRight;
          return _positionChangedEntities;
        }

        // 왼쪽에서 충분한 공간을 찾은 경우
        // (leftIndex, insertedIndex + 1) 열린 구간 안에 [leftIndex + 1, insertedIndex] 범위를 다시 배치
        // source는 재배치 범위의 마지막 항목이 됨
        if ((long)nextPos - _entities[leftIndex].Position > indexGap)
        {
          RepositionRange(leftIndex + 1, insertedIndex, _entities[leftIndex].Position, rightBoundaryPos);
          repositionRangeKind = RepositionRangeKind.ExpandedToLeft;
          return _positionChangedEntities;
        }

        // 오른쪽에서 충분한 공간을 찾은 경우
        // (insertedIndex - 1, rightIndex) 열린 구간 안에 [insertedIndex, rightIndex - 1] 범위를 다시 배치
        // source는 재배치 범위의 첫 번째 항목이 됨
        if ((long)_entities[rightIndex].Position - prevPos > indexGap)
        {
          RepositionRange(insertedIndex, rightIndex - 1, leftBoundaryPos, _entities[rightIndex].Position);
          repositionRangeKind = RepositionRangeKind.ExpandedToRight;
          return _positionChangedEntities;
        }
      }
    }

    private void AddAffectedIfPositionChanged(UserNavigationEntity entity, int newPosition)
    {
      if (TryUpdatePosition(entity, newPosition))
      {
        _positionChangedEntities.Add(entity);
      }
    }

    private static bool TryUpdatePosition(UserNavigationEntity entity, int newPosition)
    {
      if (newPosition == TemporarySourcePosition)
      {
        throw new InvalidOperationException($"임시 Position 값({TemporarySourcePosition})은 정상 Navigation Position으로 사용할 수 없습니다.");
      }

      if (entity.Position == newPosition)
      {
        return false;
      }

      entity.Position = newPosition;
      return true;
    }

    // 닫힌 구간 [startIndex, endIndex] 안의 인덱스를 가진 항목들의 Position을 재조정함
    // Position 간격은 위 구간을 열린 구간((startIndex-1, endIndex + 1))으로 확장했을 때, 양끝 인덱스의 Position(경계 Position)들을 균등 분할하여 계산함
    // 위 열린 구간에서 양끝이 존재하지 않는 인덱스가 있다면, 그곳에 null을 넣고 간격은 기본 간격으로 설정함
    private void RepositionRange(int startIndex, int endIndex, int? leftBoundaryPosition, int? rightBoundaryPosition)
    {
      ArgumentOutOfRangeException.ThrowIfLessThan(startIndex, 0, nameof(startIndex));
      ArgumentOutOfRangeException.ThrowIfLessThan(endIndex, 0, nameof(endIndex));
      ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(startIndex, Count, nameof(startIndex));
      ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(endIndex, Count, nameof(endIndex));

      if (startIndex > endIndex)
      {
        throw new ArgumentException($"잘못된 Position 재배치 범위입니다. StartIndex={startIndex}, EndIndex={endIndex}");
      }

      int rangeCount = endIndex - startIndex + 1;
      int calculatedStep = CalculatePositionStep(rangeCount, leftBoundaryPosition, rightBoundaryPosition);

      for (int offset = 0; offset < rangeCount; offset++)
      {
        long newPosition = leftBoundaryPosition switch
        {
          // 왼쪽 경계가 있으면 왼쪽에서 오른쪽으로 step씩 증가
          int => leftBoundaryPosition.Value + ((long)calculatedStep * (offset + 1)),
          // 왼쪽 경계가 없고 오른쪽 경계만 있으면 오른쪽 경계에서 왼쪽으로 step씩 감소
          null when rightBoundaryPosition is int => rightBoundaryPosition.Value - ((long)calculatedStep * (rangeCount - offset)),
          // 경계가 둘 다 없는 경우는 전체 컬렉션 재배치에 해당
          null => (long)calculatedStep * offset
        };

        AddAffectedIfPositionChanged(_entities[startIndex + offset], checked((int)newPosition));
      }
    }

    private int CalculatePositionStep(int repositionItemCount, int? leftBoundaryPosition, int? rightBoundaryPosition)
    {
      ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(repositionItemCount, 0, nameof(repositionItemCount));
      ArgumentOutOfRangeException.ThrowIfGreaterThan(repositionItemCount, Count, nameof(repositionItemCount));

      if (leftBoundaryPosition is int && rightBoundaryPosition is int)
      {
        long boundarySpan = (long)rightBoundaryPosition.Value - leftBoundaryPosition.Value;
        if (boundarySpan <= 0)
        {
          throw new InvalidOperationException($"Position 경계 순서가 올바르지 않습니다.  Left={leftBoundaryPosition.Value}, Right={rightBoundaryPosition.Value}");
        }

        long boundaryStep = boundarySpan / (repositionItemCount + 1L);

        return boundaryStep <= 0
          ? throw new InvalidOperationException($"Position 재배치 간격을 확보할 수 없습니다. Left={leftBoundaryPosition.Value}, Right={rightBoundaryPosition.Value}, RangeCount={repositionItemCount}")
          : checked((int)boundaryStep);
      }

      long totalSpan = (long)_entities[^1].Position - _entities[0].Position;
      int preferredStep = Count < 2 || totalSpan <= 0
        ? DefaultPositionStep
        : (int)Math.Clamp(totalSpan / (Count - 1L), DefaultPositionStep, MaxPositionStep);

      if (leftBoundaryPosition is int)
      {
        long maxRightwardStep = ((long)int.MaxValue - leftBoundaryPosition.Value) / repositionItemCount;
        return maxRightwardStep <= 0
          ? throw new InvalidOperationException($"오른쪽 방향으로 Position 재배치 공간이 부족합니다. Left={leftBoundaryPosition.Value}, RangeCount={repositionItemCount}")
          : checked((int)Math.Min(preferredStep, maxRightwardStep));
      }

      if (rightBoundaryPosition is int)
      {
        long maxLeftwardStep = ((long)rightBoundaryPosition.Value - (TemporarySourcePosition + 1)) / repositionItemCount;
        return maxLeftwardStep <= 0
          ? throw new InvalidOperationException($"왼쪽 방향으로 Position 재배치 공간이 부족합니다. Right={rightBoundaryPosition.Value}, RangeCount={repositionItemCount}")
          : checked((int)Math.Min(preferredStep, maxLeftwardStep));
      }

      return preferredStep;
    }
  }
}
