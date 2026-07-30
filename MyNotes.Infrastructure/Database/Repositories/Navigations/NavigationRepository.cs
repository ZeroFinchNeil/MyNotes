using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotNext;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using MyNotes.Application.Contracts.Database.Core;
using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Application.Contracts.Navigations.Persistence;
using MyNotes.Application.Contracts.Persistence;
using MyNotes.Common.Enums.Modes;
using MyNotes.Domain.Navigations;
using MyNotes.Infrastructure.Database.Constants;
using MyNotes.Infrastructure.Database.Core;
using MyNotes.Infrastructure.Database.Entities.Navigations;
using MyNotes.Infrastructure.Mappers;

namespace MyNotes.Infrastructure.Database.Repositories.Navigations;

internal partial class NavigationRepository : INavigationRepository
{
  private readonly IDbContextFactory<AppDbContext> DbContextFactory;

  public NavigationRepository(IDbContextFactory<AppDbContext> dbContextFactory)
  {
    DbContextFactory = dbContextFactory;
  }

  #region Retrieval
  public async Task<NavigationId> GenerateUniqueNavigationIdAsync(CancellationToken cancellationToken = default)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);

    NavigationId navigationId;
    do
    {
      navigationId = NavigationId.NewId();
    } while (await context.NavigationEntities.AsNoTracking().AnyAsync(e => e.Id == navigationId.Value, cancellationToken));

    return navigationId;
  }

  public async Task<IReadOnlyList<NavigationDto>> GetAllNavigationsInSiblingOrderAsync(CancellationToken cancellationToken = default)
  {
    await using AppDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken);

    var orderedNavigations = await context.NavigationEntities
      .AsNoTracking()
      .OrderBy(e => e.Parent)
      .ThenBy(e => e.Position)
      .ToListAsync(cancellationToken);

    Dictionary<Guid, CompositeNavigationViewStateEntity> compositeStates = await context.CompositeNavigationViewStateEntities
      .AsNoTracking()
      .ToDictionaryAsync(entity => entity.Id, cancellationToken);

    Dictionary<Guid, LeafNavigationViewStateEntity> leafStates = await context.LeafNavigationViewStateEntities
      .AsNoTracking()
      .ToDictionaryAsync(entity => entity.Id, cancellationToken);

    return [..orderedNavigations.Select(e =>
    {
      if (e.IsComposite)
      {
        if (compositeStates.TryGetValue(e.Id, out var compositeState))
        {
          return NavigationMappers.ToDto(e, compositeState);
        }
      }
      else
      {
        if (leafStates.TryGetValue(e.Id, out var leafState))
        {
          return NavigationMappers.ToDto(e, leafState);
        }
      }
      throw new InvalidOperationException();
    })];
  }

  public async Task<NavigationDto?> GetNavigationByIdAsync(NavigationId id, CancellationToken cancellationToken = default)
  {
    await using AppDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken);

    try
    {
      return await context.NavigationEntities
        .AsNoTracking()
        .Where(e => e.Id == id.Value)
        .Select(e => e.IsComposite)
        .SingleAsync(cancellationToken)
        ? await context.NavigationEntities
        .AsNoTracking()
        .Where(e => e.Id == id.Value && e.IsComposite)
        .Join(
          context.CompositeNavigationViewStateEntities.AsNoTracking(),
          navigation => navigation.Id,
          viewState => viewState.Id,
          (navigation, viewState) => NavigationMappers.ToDto(navigation, viewState))
        .FirstOrDefaultAsync(cancellationToken)
        : await context.NavigationEntities
        .AsNoTracking()
        .Where(e => e.Id == id.Value && !e.IsComposite)
        .Join(
          context.LeafNavigationViewStateEntities.AsNoTracking(),
          navigation => navigation.Id,
          viewState => viewState.Id,
          (navigation, viewState) => NavigationMappers.ToDto(navigation, viewState))
        .FirstOrDefaultAsync(cancellationToken);
    }
    catch
    {

    }

    return null;
  }

  public async Task<NavigationProjectionDto> GetNavigationFieldValuesAsync(NavigationId navigationId, NavigationProjectionFields navigationGetFields, CancellationToken cancellationToken = default)
  {
    await using AppDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken);

    return await context.NavigationEntities
      .AsNoTracking()
      .Where(e => e.Id == navigationId.Value)
      .Select(e => new NavigationProjectionDto()
      {
        Id = navigationId,
        ParentId = navigationGetFields.HasFlag(NavigationProjectionFields.ParentId) ? NavigationId.Create(e.Parent) : Optional<NavigationId>.None,
        IsComposite = navigationGetFields.HasFlag(NavigationProjectionFields.IsComposite) ? e.IsComposite : Optional<bool>.None,
        Icon = navigationGetFields.HasFlag(NavigationProjectionFields.Icon) ? e.Icon : Optional<int>.None,
        Title = navigationGetFields.HasFlag(NavigationProjectionFields.Title) ? e.Title : Optional<string>.None,
        IsDeleted = navigationGetFields.HasFlag(NavigationProjectionFields.IsDeleted) ? e.IsDeleted : Optional<bool>.None,
      })
      .FirstOrDefaultAsync(cancellationToken)
      ?? new()
      {
        Id = navigationId
      };
  }

  public async Task<bool> IsDescendantOfAsync(NavigationId possibleDescendantId, NavigationId possibleAncestorId, CancellationToken cancellationToken = default)
  {
    await using AppDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken);

    Guid? current = possibleDescendantId.Value;
    Guid rootId = NavigationId.UserRoot.Value;

    while (current is not null)
    {
      if (current == possibleAncestorId.Value)
      {
        return true;
      }

      if (current == rootId)
      {
        return false;
      }

      current = await context.NavigationEntities
        .AsNoTracking()
        .Where(e => e.Id == current)
        .Select(e => (Guid?)e.Parent)
        .SingleOrDefaultAsync(cancellationToken);
    }

    return false;
  }
  #endregion

  public async Task AddNavigationAsync(NavigationDto navigationDto, NavigationId targetNavigationId, NavigationInsertPosition insertPosition, IAppDbTransactionContext appDbTransactionContext, CancellationToken cancellationToken = default)
  {
    Guid sourceId = navigationDto.Id.Value;
    Guid targetId = targetNavigationId.Value;
    Guid parentId = navigationDto.ParentId.Value;

    if ((insertPosition is NavigationInsertPosition.FirstChild or NavigationInsertPosition.LastChild)
        && targetId != parentId)
    {
      throw new InvalidOperationException();
    }

    var context = appDbTransactionContext.DbContext as AppDbContext
      ?? throw new InvalidOperationException($"지원하지 않는 DbContext 타입입니다. Expected: {typeof(AppDbContext).FullName}, Actual: {appDbTransactionContext.DbContext.GetType().FullName}");

    try
    {
      NavigationPositionItem sourceItem = new(sourceId, parentId, NavigationEntitySettings.TemporaryPosition);

      List<NavigationPositionItem> siblingItems = await context.NavigationEntities
        .AsNoTracking()
        .Where(e => e.Parent == parentId)
        .OrderBy(e => e.Position)
        .Select(e => new NavigationPositionItem(e.Id, e.Parent, e.Position))
        .ToListAsync(cancellationToken);

      // sourceItem을 siblingItems에 삽입하고 position 재조정 후 재조정된 siblingItems 변경 사항 DB에 반영 
      await RepositionInsertedNavigationAsync(context, siblingItems, sourceItem, targetId, insertPosition, cancellationToken);

      NavigationEntity entity = NavigationMappers.ToEntity(navigationDto, sourceItem.Position);

      await context.NavigationEntities.AddAsync(entity, cancellationToken);

      if (navigationDto.IsComposite)
      {
        var compositeViewStateEntity = NavigationMappers.ToEntity((CompositeNavigationViewStateDto)navigationDto.ViewStateDto);
        await context.CompositeNavigationViewStateEntities.AddAsync(compositeViewStateEntity, cancellationToken);
      }
      else
      {
        var leafViewStateEntity = NavigationMappers.ToEntity((LeafNavigationViewStateDto)navigationDto.ViewStateDto);
        await context.LeafNavigationViewStateEntities.AddAsync(leafViewStateEntity, cancellationToken);
      }
    }
    catch
    {
      throw;
    }
  }

  public async Task<IReadOnlyList<NavigationProjectionDto>> MoveNavigationAsync(NavigationId sourceNavigationId, NavigationId targetNavigationId, NavigationInsertPosition insertPosition, IAppDbTransactionContext appDbTransactionContext, CancellationToken cancellationToken = default)
  {
    // Source Navigation을 포함하여 Position에 영향받는 모든 Navigation들의 Id, Parent, Position을 담아서 반환(이 때 반환되는 모든 Navigation의 Parent 속성 값은 모두 일치해야 함).
    List<NavigationProjectionDto> resultDtos = new();

    Guid sourceId = sourceNavigationId.Value;
    Guid targetId = targetNavigationId.Value;

    if (sourceId == targetId)
    {
      throw new ArgumentException($"Source Navigation과 Target Navigation은 동일할 수 없습니다. SourceId={sourceId}, TargetId={targetId}", nameof(sourceNavigationId));
    }

    var context = appDbTransactionContext.DbContext as AppDbContext
      ?? throw new InvalidOperationException($"지원하지 않는 DbContext 타입입니다. Expected: {typeof(AppDbContext).FullName}, Actual: {appDbTransactionContext.DbContext.GetType().FullName}");

    try
    {
      var sourceItem = await context.NavigationEntities
        .AsNoTracking()
        .Where(e => e.Id == sourceId)
        .Select(e => new NavigationPositionItem(e.Id, e.Parent, e.Position))
        .FirstOrDefaultAsync(cancellationToken)
        ?? throw new InvalidOperationException($"Source Navigation을 찾을 수 없습니다. Id={sourceId}");

      var targetItem = await context.NavigationEntities
        .AsNoTracking()
        .Where(e => e.Id == targetId)
        .Select(e => new NavigationPositionItem(e.Id, e.Parent, e.Position))
        .FirstOrDefaultAsync(cancellationToken)
        ?? throw new InvalidOperationException($"Target Navigation을 찾을 수 없습니다. Id={targetId}");

      Guid oldSourceParent = sourceItem.Parent;
      Guid newSourceParent = insertPosition switch
      {
        NavigationInsertPosition.Before or NavigationInsertPosition.After => targetItem.Parent,
        NavigationInsertPosition.FirstChild or NavigationInsertPosition.LastChild => targetItem.Id,
        _ => throw new NotSupportedException($"지원하지 않는 Navigation 삽입 위치입니다. InsertPosition={insertPosition}")
      };
      int oldSourcePosition = sourceItem.Position;

      List<NavigationPositionItem> siblingItems = await context.NavigationEntities
        .AsNoTracking()
        .Where(e => e.Parent == newSourceParent)
        .OrderBy(e => e.Position)
        .Select(e => new NavigationPositionItem(e.Id, e.Parent, e.Position))
        .ToListAsync(cancellationToken);

      siblingItems.RemoveAll(e => e.Id == sourceId);

      // sourceEntity의 Parent를 변경하고 임시 Position 설정 후 DB에 반영
      if (!await EnsureTemporarySourceSlotIsAvailableAsync(context, newSourceParent))
      {
        throw new InvalidOperationException($"source Navigation 임시 Position이 이미 사용 중입니다. Parent={newSourceParent}, Position={NavigationEntitySettings.TemporaryPosition}");
      }
      sourceItem.Parent = newSourceParent;
      sourceItem.Position = NavigationEntitySettings.TemporaryPosition;

      // 
      int sourceTemporaryMoveRows = await context.NavigationEntities
        .Where(e => e.Id == sourceId && e.Parent == oldSourceParent && e.Position == oldSourcePosition)
        .ExecuteUpdateAsync(setters => setters
          .SetProperty(e => e.Parent, sourceItem.Parent)
          .SetProperty(e => e.Position, sourceItem.Position), cancellationToken);

      if (sourceTemporaryMoveRows != 1)
      {
        throw new InvalidOperationException($"source Navigation 임시 Position 반영에 실패했습니다. Id={sourceItem.Id}, AffectedRows={sourceTemporaryMoveRows}");
      }

      // sourceItem을 siblingItems에 삽입하고 position 재조정 후 재조정된 siblingItems 변경 사항 DB에 반영 
      await RepositionInsertedNavigationAsync(context, siblingItems, sourceItem, targetId, insertPosition, cancellationToken);

      // sourceEntity의 정상 Position DB 반영
      var sourceFinalMoveRows = await context.NavigationEntities
        .Where(e => e.Id == sourceItem.Id
                    && e.Parent == newSourceParent
                    && e.Position == NavigationEntitySettings.TemporaryPosition)
        .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.Position, sourceItem.Position), cancellationToken);

      if (sourceFinalMoveRows != 1)
      {
        throw new InvalidOperationException($"source Navigation 최종 Position 반영에 실패했습니다. source가 임시 위치에 없거나 동시 변경되었을 수 있습니다. Id={sourceItem.Id}, Parent={newSourceParent}, ExpectedPosition={NavigationEntitySettings.TemporaryPosition}, NewPosition={sourceItem.Position}, AffectedRows={sourceFinalMoveRows}");
      }

      foreach (var siblingEntity in siblingItems)
      {
        resultDtos.Add(new NavigationProjectionDto()
        {
          Id = NavigationId.Create(siblingEntity.Id),
          ParentId = NavigationId.Create(siblingEntity.Parent)
        });
      }

      return resultDtos;
    }
    catch
    {
      throw;
    }
  }

  private static async Task RepositionInsertedNavigationAsync(AppDbContext context, List<NavigationPositionItem> siblingItems, NavigationPositionItem sourceItem, Guid targetId, NavigationInsertPosition insertPosition, CancellationToken cancellationToken)
  {
    Guid sourceId = sourceItem.Id;
    Guid parentId = sourceItem.Parent;

    int insertedIndex = insertPosition switch
    {
      NavigationInsertPosition.Before => siblingItems.FindIndex(item => item.Id == targetId),
      NavigationInsertPosition.After => siblingItems.FindIndex(item => item.Id == targetId) + 1,
      NavigationInsertPosition.FirstChild => 0,
      NavigationInsertPosition.LastChild => siblingItems.Count,
      _ => throw new NotSupportedException($"지원하지 않는 Navigation 삽입 위치입니다. InsertPosition={insertPosition}")
    };

    if (insertedIndex < 0)
    {
      throw new InvalidOperationException($"Target Navigation이 이동 후 형제 목록에 없습니다. TargetId={targetId}, ParentId={parentId}, InsertPosition={insertPosition}");
    }

    siblingItems.Insert(insertedIndex, sourceItem);

    Dictionary<Guid, int> originalSiblingPositionById = siblingItems.ToDictionary(e => e.Id, e => e.Position);
    var affectedEntities = NavigationRepositioning.RepositionFromInsertedNavigation(siblingItems, insertedIndex, out RepositionRangeKind siblingsRepositionRangeKind);
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

    async Task UpdateSiblingPositionAsync(NavigationPositionItem siblingItem)
    {
      if (!originalSiblingPositionById.TryGetValue(siblingItem.Id, out var affectedEntityOriginalPosition))
      {
        throw new InvalidOperationException($"Navigation의 원래 Position을 찾을 수 없습니다. Id={siblingItem.Id}, Parent={parentId}");
      }
      int affectedRows = await context.NavigationEntities
        .Where(e => e.Id == siblingItem.Id
                    && e.Parent == parentId
                    && e.Position == affectedEntityOriginalPosition)
        .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.Position, siblingItem.Position), cancellationToken);

      if (affectedRows != 1)
      {
        throw new InvalidOperationException($"Navigation Position 업데이트에 실패했습니다. 형제 Navigation의 Position이 동시 변경되었을 수 있습니다. Id={siblingItem.Id}, Parent={parentId}, ExpectedPosition={affectedEntityOriginalPosition}, NewPosition={siblingItem.Position}, AffectedRows={affectedRows}");
      }
    }
  }

  private static async Task<bool> EnsureTemporarySourceSlotIsAvailableAsync(AppDbContext context, Guid parentId) =>
    !await context.NavigationEntities.AsNoTracking().AnyAsync(e => e.Parent == parentId && e.Position == NavigationEntitySettings.TemporaryPosition);

  public async Task<PersistenceMutationStatus> UpdateNavigationAsync(NavigationPatchDto patchDto, CancellationToken cancellationToken = default)
  {
    if (patchDto.IsEmpty)
    {
      return PersistenceMutationStatus.Unchanged;
    }

    await using AppDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken);
    await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
    try
    {
      if (await context.NavigationEntities.Where(e => e.Id == patchDto.Id.Value).SingleOrDefaultAsync(cancellationToken) is NavigationEntity entity)
      {
        if (patchDto.ParentId.TryGet(out var parentId) && entity.Parent != parentId.Value)
        {
          entity.Parent = parentId.Value;
        }
        if (patchDto.Icon.TryGet(out var icon) && entity.Icon != icon)
        {
          entity.Icon = icon;
        }
        if (patchDto.Title.TryGet(out var title) && entity.Title != title)
        {
          entity.Title = title;
        }
        if (patchDto.IsDeleted.TryGet(out var isDeleted) && entity.IsDeleted != isDeleted)
        {
          entity.IsDeleted = isDeleted;
        }

        if (await context.SaveChangesAsync(cancellationToken) > 0)
        {
          await transaction.CommitAsync(cancellationToken);
          return PersistenceMutationStatus.Applied;
        }

        return PersistenceMutationStatus.Unchanged;
      }

      return PersistenceMutationStatus.NotFound;
    }
    catch
    {
      await transaction.RollbackAsync(cancellationToken);
      return PersistenceMutationStatus.Failed;
    }
  }

  public async Task<PersistenceMutationStatus> UpdateNavigationViewStateAsync(NavigationViewStatePatchDto patchDto, CancellationToken cancellationToken = default)
  {
    if (patchDto.IsEmpty)
    {
      return PersistenceMutationStatus.Unchanged;
    }

    await using AppDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken);
    await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken);

    try
    {
      int updateResult = 0;
      switch (patchDto)
      {
        case CompositeNavigationViewStatePatchDto compositeDto:
          updateResult = await context.EnqueueOperationAsync(
            operation: () => context.CompositeNavigationViewStateEntities
              .Where(e => e.Id == compositeDto.Id.Value)
              .ExecuteUpdate(setters =>
              {
                if (compositeDto.IsExpanded.TryGet(out var isExpanded))
                {
                  setters.SetProperty(e => e.IsExpanded, isExpanded);
                }
              }),
            defaultValue: 0,
            fallbackValue: 0,
            cancellationToken: cancellationToken);
          break;
        case LeafNavigationViewStatePatchDto leafDto:
          updateResult = await context.EnqueueOperationAsync(
            operation: () => context.LeafNavigationViewStateEntities
              .Where(e => e.Id == leafDto.Id.Value)
              .ExecuteUpdate(setters =>
              {
                if (leafDto.NoteSortKey.TryGet(out var noteSortKey))
                {
                  setters.SetProperty(e => e.NoteSortKey, (int)noteSortKey);
                }
                if (leafDto.NoteSortDirection.TryGet(out var noteSortDirection))
                {
                  setters.SetProperty(e => e.NoteSortDirection, (int)noteSortDirection);
                }
                if (leafDto.PreviewLayoutType.TryGet(out var previewLayoutType))
                {
                  setters.SetProperty(e => e.PreviewLayoutType, (int)previewLayoutType);
                }
                if (leafDto.PreviewTileSize.TryGet(out var previewTileSize))
                {
                  setters.SetProperty(e => e.PreviewTileSize, (int)previewTileSize);
                }
                if (leafDto.PreviewTileRatio.TryGet(out var previewTileRatio))
                {
                  setters.SetProperty(e => e.PreviewTileRatio, (int)previewTileRatio);
                }
              }),
            defaultValue: 0,
            fallbackValue: 0,
            cancellationToken: cancellationToken);
          break;
      }
      if (updateResult > 0)
      {
        await transaction.CommitAsync(cancellationToken);
        return PersistenceMutationStatus.Applied;
      }
      return PersistenceMutationStatus.NotFound;
    }
    catch
    {
      await transaction.RollbackAsync(cancellationToken);
      return PersistenceMutationStatus.Failed;
    }
  }

  public async Task<PersistenceMutationStatus> DeleteNavigationAsync(NavigationId navigationId, DeleteMode deleteMode, CancellationToken cancellationToken = default)
  {
    await using AppDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken);
    await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken);

    try
    {
      var id = navigationId.Value;
      int result = deleteMode switch
      {
        DeleteMode.MoveToTrash => await context.NavigationEntities.Where(e => e.Id == id)
          .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.IsDeleted, true), cancellationToken),
        DeleteMode.Permanent => await context.NavigationEntities.Where(e => e.Id == id)
          .ExecuteDeleteAsync(cancellationToken),
        _ => 0
      };

      if (result > 0)
      {
        await transaction.CommitAsync(cancellationToken);
        return PersistenceMutationStatus.Applied;
      }
      return PersistenceMutationStatus.NotFound;
    }
    catch
    {
      await transaction.RollbackAsync(cancellationToken);
      return PersistenceMutationStatus.Failed;
    }
  }
}

internal partial class NavigationRepository
{
  /// <summary>
  /// source 삽입 후 Position 재계산이 어떤 범위에서 발생했는지 나타냅니다.
  /// </summary>
  private enum RepositionRangeKind { SourceOnly, ExpandedToLeft, ExpandedToRight }

  private sealed class NavigationPositionItem : IEquatable<NavigationPositionItem>
  {
    public NavigationPositionItem() { }

    [SetsRequiredMembers]
    public NavigationPositionItem(Guid id, Guid parent, int position)
    {
      Id = id;
      Parent = parent;
      Position = position;
    }

    public required Guid Id { get; init; }

    public required Guid Parent { get; set; }

    public required int Position { get; set; }

    public bool Equals(NavigationPositionItem? other) => other is not null && other.Id == Id;

    public override bool Equals(object? obj) => Equals(obj as NavigationPositionItem);

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(NavigationPositionItem i1, NavigationPositionItem i2) => i1.Equals(i2);

    public static bool operator !=(NavigationPositionItem i1, NavigationPositionItem i2) => !i1.Equals(i2);
  }

  private sealed class NavigationRepositioning
  {
    private readonly IList<NavigationPositionItem> _items;
    private readonly List<NavigationPositionItem> _positionChangedItems = new();

    private int Count => _items.Count;

    private NavigationRepositioning(IList<NavigationPositionItem> items)
    {
      ArgumentNullException.ThrowIfNull(items);
      _items = items;
    }

    public static IReadOnlyList<NavigationPositionItem> RepositionFromInsertedNavigation(IList<NavigationPositionItem> items, int insertedIndex, out RepositionRangeKind repositionRangeKind) => new NavigationRepositioning(items)
      .RepositionFromInsertedNavigation(insertedIndex, out repositionRangeKind);

    private IReadOnlyList<NavigationPositionItem> RepositionFromInsertedNavigation(int insertedIndex, out RepositionRangeKind repositionRangeKind)
    {
      ArgumentOutOfRangeException.ThrowIfLessThan(insertedIndex, 0, nameof(insertedIndex));
      ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(insertedIndex, Count, nameof(insertedIndex));

      repositionRangeKind = RepositionRangeKind.SourceOnly;

      // 추가한 후 컬렉션 항목이 하나라면 추가한 항목의 Position은 0임
      if (Count == 1)
      {
        AddAffectedIfPositionChanged(_items[insertedIndex], 0);
        return _positionChangedItems;
      }

      // 항목이 맨 앞에 추가되었다면, 기존 첫 번째 항목의 Position보다 작은 Position을 계산하여 설정
      if (insertedIndex == 0)
      {
        AddAffectedIfPositionChanged(_items[0], checked(_items[1].Position - CalculatePositionStep(1, null, _items[1].Position)));
        return _positionChangedItems;
      }

      // 항목이 맨 뒤에 추가되었다면, 기존 맨 마지막 항목의 Position보다 큰 Position을 계산하여 설정
      if (insertedIndex == Count - 1)
      {
        AddAffectedIfPositionChanged(_items[^1], checked(_items[^2].Position + CalculatePositionStep(1, _items[^2].Position, null)));
        return _positionChangedItems;
      }

      // 후보 Position이 앞뒤와 겹치지 않는다면 그대로 확정
      int prevPos = _items[insertedIndex - 1].Position;
      int nextPos = _items[insertedIndex + 1].Position;
      long posDiff = (long)nextPos - prevPos;

      if (posDiff < 0)
      {
        throw new InvalidOperationException($"Position 순서가 올바르지 않습니다. prevPos={prevPos}, nextPos={nextPos}");
      }

      // Position 변경된 범위 인덱스 양끝 중에 insertedIndex가 있으면
      // 재조정 간격(step)을 계산하기 위해 insertedIndex의 왼쪽 혹은 오른쪽 항목의 Position을 알아야 함
      // insertedIndex <= 0 이나 insertedIndex >= Count - 1인 상황은 위에서 걸러짐
      int? leftBoundaryPos = _items[insertedIndex - 1].Position;
      int? rightBoundaryPos = _items[insertedIndex + 1].Position;

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
          return _positionChangedItems;
        }

        // 오른쪽 끝에 도달했을 경우
        // 오른쪽 boundary가 없으므로 [insertedIndex, Count - 1] 범위를 왼쪽 boundary 기준으로 다시 배치
        // source는 재배치 범위의 첫 번째 항목이 됨
        if (rightIndex >= Count)
        {
          RepositionRange(insertedIndex, Count - 1, leftBoundaryPos, null);
          repositionRangeKind = RepositionRangeKind.ExpandedToRight;
          return _positionChangedItems;
        }

        // 왼쪽에서 충분한 공간을 찾은 경우
        // (leftIndex, insertedIndex + 1) 열린 구간 안에 [leftIndex + 1, insertedIndex] 범위를 다시 배치
        // source는 재배치 범위의 마지막 항목이 됨
        if ((long)nextPos - _items[leftIndex].Position > indexGap)
        {
          RepositionRange(leftIndex + 1, insertedIndex, _items[leftIndex].Position, rightBoundaryPos);
          repositionRangeKind = RepositionRangeKind.ExpandedToLeft;
          return _positionChangedItems;
        }

        // 오른쪽에서 충분한 공간을 찾은 경우
        // (insertedIndex - 1, rightIndex) 열린 구간 안에 [insertedIndex, rightIndex - 1] 범위를 다시 배치
        // source는 재배치 범위의 첫 번째 항목이 됨
        if ((long)_items[rightIndex].Position - prevPos > indexGap)
        {
          RepositionRange(insertedIndex, rightIndex - 1, leftBoundaryPos, _items[rightIndex].Position);
          repositionRangeKind = RepositionRangeKind.ExpandedToRight;
          return _positionChangedItems;
        }
      }
    }

    private void AddAffectedIfPositionChanged(NavigationPositionItem item, int newPosition)
    {
      if (TryUpdatePosition(item, newPosition))
      {
        _positionChangedItems.Add(item);
      }
    }

    private static bool TryUpdatePosition(NavigationPositionItem item, int newPosition)
    {
      if (newPosition == NavigationEntitySettings.TemporaryPosition)
      {
        throw new InvalidOperationException($"임시 Position 값({NavigationEntitySettings.TemporaryPosition})은 정상 Navigation Position으로 사용할 수 없습니다.");
      }

      if (item.Position == newPosition)
      {
        return false;
      }

      item.Position = newPosition;
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

        AddAffectedIfPositionChanged(_items[startIndex + offset], checked((int)newPosition));
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

        Console.WriteLine("{0}: {1}", "boundarySpan", boundarySpan);
        Console.WriteLine("{0}: {1}", "boundaryStep", boundaryStep);

        return boundaryStep <= 0
          ? throw new InvalidOperationException($"Position 재배치 간격을 확보할 수 없습니다. Left={leftBoundaryPosition.Value}, Right={rightBoundaryPosition.Value}, RangeCount={repositionItemCount}")
          : checked((int)boundaryStep);
      }

      int includedItemCount = 0;
      int minPosition = int.MaxValue;
      int maxPosition = int.MinValue;

      foreach (NavigationPositionItem item in _items)
      {
        if (item.Position == NavigationEntitySettings.TemporaryPosition)
        {
          continue;
        }

        includedItemCount++;
        minPosition = Math.Min(minPosition, item.Position);
        maxPosition = Math.Max(maxPosition, item.Position);
      }

      long totalSpan = (long)maxPosition - minPosition;

      int preferredStep = includedItemCount <= 1 || totalSpan <= 0
       ? NavigationEntitySettings.DefaultPositionStep
       : (int)Math.Clamp(totalSpan / (includedItemCount - 1L), NavigationEntitySettings.DefaultPositionStep, NavigationEntitySettings.MaxPositionStep);

      Console.WriteLine("{0}: {1}", "totalSpan", totalSpan);
      Console.WriteLine("{0}: {1}", "preferredStep", preferredStep);

      if (leftBoundaryPosition is int)
      {
        long maxRightwardStep = ((long)int.MaxValue - leftBoundaryPosition.Value) / repositionItemCount;
        return maxRightwardStep <= 0
          ? throw new InvalidOperationException($"오른쪽 방향으로 Position 재배치 공간이 부족합니다. Left={leftBoundaryPosition.Value}, RangeCount={repositionItemCount}")
          : checked((int)Math.Min(preferredStep, maxRightwardStep));
      }

      if (rightBoundaryPosition is int)
      {
        long maxLeftwardStep = ((long)rightBoundaryPosition.Value - (int.MinValue + 1)) / repositionItemCount;
        return maxLeftwardStep <= 0
          ? throw new InvalidOperationException($"왼쪽 방향으로 Position 재배치 공간이 부족합니다. Right={rightBoundaryPosition.Value}, RangeCount={repositionItemCount}")
          : checked((int)Math.Min(preferredStep, maxLeftwardStep));
      }

      return preferredStep;
    }
  }
}