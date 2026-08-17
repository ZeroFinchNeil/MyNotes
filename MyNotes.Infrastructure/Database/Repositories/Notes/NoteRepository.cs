using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using DotNext;

using Microsoft.EntityFrameworkCore;

using MyNotes.Application.Contracts.Database.Core;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Notes.Persistence;
using MyNotes.Application.Contracts.Persistence;
using MyNotes.Application.Contracts.Querying.Conditions;
using MyNotes.Application.Contracts.Querying.Models;
using MyNotes.Common.Enums.Modes;
using MyNotes.Common.Expressions;
using MyNotes.Common.Structures;
using MyNotes.Domain.Navigations;
using MyNotes.Domain.Notes;
using MyNotes.Infrastructure.Database.Core;
using MyNotes.Infrastructure.Database.Entities.Notes;
using MyNotes.Infrastructure.Mappers;

namespace MyNotes.Infrastructure.Database.Repositories.Notes;

internal class NoteRepository : INoteRepository
{
  private readonly IDbContextFactory<AppDbContext> DbContextFactory;

  public NoteRepository(IDbContextFactory<AppDbContext> dbContextFactory)
  {
    DbContextFactory = dbContextFactory;
  }

  public async Task<NoteId> GenerateUniqueNoteIdAsync(CancellationToken cancellationToken = default)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);

    NoteId noteId;
    do
    {
      noteId = NoteId.NewId();
    } while (await context.NoteEntities.AsNoTracking().AnyAsync(e => e.Id == noteId.Value, cancellationToken));

    return noteId;
  }

  public async Task<NoteDto?> GetNoteByIdAsync(NoteId noteId, CancellationToken cancellationToken = default)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);
    return await context.NoteEntities
      .AsNoTracking()
      .Where(e => e.Id == noteId.Value)
      .Join(
        context.NoteViewStateEntities.AsNoTracking(),
        outer => outer.Id,
        inner => inner.Id,
        (outer, inner) => NoteMappers.ToDto(outer, inner))
      .FirstOrDefaultAsync(cancellationToken);
  }

  public async Task<NoteViewStateDto?> GetNoteViewStateByIdAsync(NoteId noteId, CancellationToken cancellationToken)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);
    return await context.NoteViewStateEntities
      .AsNoTracking()
      .Where(e => e.Id == noteId.Value)
      .Select(e => NoteMappers.ToDto(e))
      .FirstOrDefaultAsync(cancellationToken);
  }

  public async Task<IReadOnlyList<NoteDto>> GetNotesByParentAsync(NavigationId navigationId, bool includeDeleted, CancellationToken cancellationToken)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);
    Expression<Func<NoteEntity, bool>> predicate = includeDeleted
      ? e => e.Navigation == navigationId.Value
      : e => e.Navigation == navigationId.Value && e.IsDeleted == false;
    return await context.NoteEntities
      .AsNoTracking()
      .Where(predicate)
      .Join(
        context.NoteViewStateEntities.AsNoTracking(),
        outer => outer.Id,
        inner => inner.Id,
        (outer, inner) => NoteMappers.ToDto(outer, inner))
      .ToListAsync(cancellationToken);
  }

  public async Task<NoteProjectionDto> GetNoteFieldValuesAsync(NoteId noteId, NoteProjectionFields noteGetFields, CancellationToken cancellationToken = default)
  {
    await using AppDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken);

    return await context.NoteEntities
      .AsNoTracking()
      .Where(e => e.Id == noteId.Value)
      .Select(e => new NoteProjectionDto()
      {
        Id = noteId,
        NavigationId = noteGetFields.HasFlag(NoteProjectionFields.NavigationId)
          ? e.Navigation.HasValue
            ? new(NavigationId.Create(e.Navigation.Value)) : new(null)
          : Optional<NavigationId?>.None,
        Created = noteGetFields.HasFlag(NoteProjectionFields.Created) ? new(e.Created) : Optional<DateTimeOffset>.None,
        Modified = noteGetFields.HasFlag(NoteProjectionFields.Modified) ? new(e.Modified) : Optional<DateTimeOffset>.None,
        Title = noteGetFields.HasFlag(NoteProjectionFields.Title) ? new(e.Title) : Optional<string>.None,
        Body = noteGetFields.HasFlag(NoteProjectionFields.Body) ? new(e.Body) : Optional<string>.None,
        BackgroundColor = noteGetFields.HasFlag(NoteProjectionFields.BackgroundColor) ? new(e.BackgroundColor) : Optional<string>.None,
        IsBookmarked = noteGetFields.HasFlag(NoteProjectionFields.IsBookmarked) ? new(e.IsBookmarked) : Optional<bool>.None,
        IsDeleted = noteGetFields.HasFlag(NoteProjectionFields.IsDeleted) ? new(e.IsDeleted) : Optional<bool>.None,
      })
      .FirstOrDefaultAsync(cancellationToken)
      ?? new NoteProjectionDto()
      {
        Id = noteId
      };
  }

  public async Task<IReadOnlyList<NoteDto>> FindNotesAsync(NoteFilterDto noteFilterDto, CancellationToken cancellationToken)
  {
    //todo: DB에서 쿼리 조건에 따른 노트 가져오기
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);

    var expressions = MakeExpressions(noteFilterDto);
    if (expressions.Count == 0)
    {

    }

    var expression = noteFilterDto.AggregationMode switch
    {
      AggregationMode.All => expressions.AndAll(),
      AggregationMode.Any => expressions.OrAll(),
      _ => throw new InvalidOperationException()
    };

    return await context.NoteEntities
      .AsNoTracking()
      .Where(expression)
      .Join(
        context.NoteViewStateEntities.AsNoTracking(),
        outer => outer.Id,
        inner => inner.Id,
        (outer, inner) => NoteMappers.ToDto(outer, inner))
      .ToListAsync(cancellationToken);
  }

  private static Expression<Func<NoteEntity, bool>> CreateExpression<T>(ComparisonQueryCondition<T> queryCondition, Expression<Func<NoteEntity, T>> valueSelector) where T : IComparable<T>
  {
    var parameter = valueSelector.Parameters[0];
    var left = valueSelector.Body;
    var right = Expression.Constant(queryCondition.Target, typeof(T));

    Expression body = queryCondition.Condition switch
    {
      ComparisonOperator.EqualTo => Expression.Equal(left, right),
      ComparisonOperator.NotEqualTo => Expression.NotEqual(left, right),
      ComparisonOperator.LessThan => Expression.LessThan(left, right),
      ComparisonOperator.LessThanOrEqualTo => Expression.LessThanOrEqual(left, right),
      ComparisonOperator.GreaterThan => Expression.GreaterThan(left, right),
      ComparisonOperator.GreaterThanOrEqualTo => Expression.GreaterThanOrEqual(left, right),
      _ => throw new InvalidOperationException("지원하지 않는 비교 연산자입니다.")
    };

    return Expression.Lambda<Func<NoteEntity, bool>>(body, parameter);
  }

  private static Expression<Func<NoteEntity, bool>> CreateExpression<T>(EqualityQueryCondition<T> queryCondition, Expression<Func<NoteEntity, T>> valueSelector) where T : notnull
  {
    var parameter = valueSelector.Parameters[0];
    var left = valueSelector.Body;
    var right = Expression.Constant(queryCondition.Target, typeof(T));

    Expression body = queryCondition.Condition switch
    {
      EqualityMatchType.Equals => Expression.Equal(left, right),
      EqualityMatchType.NotEquals => Expression.NotEqual(left, right),
      _ => throw new InvalidOperationException("지원하지 않는 비교 연산자입니다.")
    };

    return Expression.Lambda<Func<NoteEntity, bool>>(body, parameter);
  }

  private static Expression<Func<NoteEntity, bool>> CombineExpressions<T>(QueryConditionSet<ComparisonQueryCondition<T>>? queryConditionSet, Expression<Func<NoteEntity, T>> valueSelector) where T : IComparable<T>
  {
    var conditions = queryConditionSet ?? throw new ArgumentNullException("", nameof(queryConditionSet));

    List<Expression<Func<NoteEntity, bool>>> expressions = new();
    foreach (var createdCondition in conditions.Conditions)
    {
      expressions.Add(CreateExpression(createdCondition, valueSelector));
    }

    return conditions.ConditionOperator switch
    {
      JoinOperator.And => expressions.AndAll(),
      JoinOperator.Or => expressions.OrAll(),
      _ => throw new InvalidOperationException()
    };
  }

  public static IReadOnlyList<Expression<Func<NoteEntity, bool>>> MakeExpressions(NoteFilterDto filter)
  {
    var noteFindFields = filter.NoteFindFields;

    if (noteFindFields == NoteFindFields.None)
    {
      throw new ArgumentException("", nameof(filter));
    }

    List<Expression<Func<NoteEntity, bool>>> expressions = new();
    if (noteFindFields.HasFlag(NoteFindFields.NoteIdCondition))
    {
      var noteIdCondition = filter.NoteIdCondition ?? throw new ArgumentException("", nameof(filter));
      expressions.Add(CreateExpression(noteIdCondition, e => e.Id));
    }
    if (noteFindFields.HasFlag(NoteFindFields.ParentIdCondition))
    {
      var parentIdCondition = filter.ParentIdCondition ?? throw new ArgumentException("", nameof(filter));
      expressions.Add(CreateExpression(parentIdCondition, e => e.Id));
    }
    if (noteFindFields.HasFlag(NoteFindFields.TitleConditions))
    {

    }
    if (noteFindFields.HasFlag(NoteFindFields.CreatedConditions))
    {
      var createdConditions = filter.CreatedConditions ?? throw new ArgumentException("", nameof(filter));
      expressions.Add(CombineExpressions(createdConditions, e => e.Created));
    }
    if (noteFindFields.HasFlag(NoteFindFields.ModifiedConditions))
    {
      var modifiedConditions = filter.ModifiedConditions ?? throw new ArgumentException("", nameof(filter));
      expressions.Add(CombineExpressions(modifiedConditions, e => e.Modified));
    }
    if (noteFindFields.HasFlag(NoteFindFields.BackgroundColorConditions))
    {
    }
    if (noteFindFields.HasFlag(NoteFindFields.BookmarkedCondition))
    {
      var bookmarkedCondition = filter.BookmarkedCondition ?? throw new ArgumentException("", nameof(filter));
      expressions.Add(CreateExpression(bookmarkedCondition, e => e.IsBookmarked));
    }
    if (noteFindFields.HasFlag(NoteFindFields.DeletedCondition))
    {
      var deletedCondition = filter.DeletedCondition ?? throw new ArgumentException("", nameof(filter));
      expressions.Add(CreateExpression(deletedCondition, e => e.IsDeleted));
    }

    return expressions;
  }

  public async Task AddNoteAsync(NoteDto noteDto, IAppDbTransactionContext appDbTransactionContext, CancellationToken cancellationToken = default)
  {
    var context = appDbTransactionContext.DbContext as AppDbContext
      ?? throw new InvalidOperationException($"지원하지 않는 DbContext 타입입니다. Expected: {typeof(AppDbContext).FullName}, Actual: {appDbTransactionContext.DbContext.GetType().FullName}");

    try
    {
      NoteEntity noteEntity = NoteMappers.ToEntity(noteDto);
      NoteViewStateEntity viewStateEntity = NoteMappers.ToEntity(noteDto.ViewStateDto);

      await context.NoteEntities.AddAsync(noteEntity, cancellationToken);
      await context.NoteViewStateEntities.AddAsync(viewStateEntity, cancellationToken);
    }
    catch
    {
      throw;
    }
  }

  public async Task<PersistenceMutationStatus> UpdateNoteAsync(NotePatchDto notePatchDto, DateTimeOffset modified, CancellationToken cancellationToken = default)
  {
    if (notePatchDto.IsEmpty)
    {
      return PersistenceMutationStatus.Unchanged;
    }

    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);
    await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

    try
    {
      if (await context.NoteEntities.Where(e => e.Id == notePatchDto.Id.Value).SingleOrDefaultAsync(cancellationToken) is NoteEntity entity)
      {
        if (entity.Modified > modified)
        {
          return PersistenceMutationStatus.Expired;
        }

        if (notePatchDto.NavigationId.TryGetSpecifiedValue(out var navigationId) && entity.Navigation != navigationId.Value)
        {
          entity.Navigation = navigationId.Value;
        }
        if (notePatchDto.Title.TryGet(out var title) && entity.Title != title)
        {
          entity.Title = title;
        }
        if (notePatchDto.Body.TryGet(out var body) && entity.Body != body)
        {
          entity.Body = body;
        }
        if (notePatchDto.BackgroundColor.TryGet(out var backgroundColor) && entity.BackgroundColor != backgroundColor)
        {
          entity.BackgroundColor = backgroundColor;
        }
        if (notePatchDto.BackgroundImagePath.TryGet(out var backgroundImagePath) && entity.BackgroundImagePath != backgroundImagePath)
        {
          entity.BackgroundImagePath = backgroundImagePath;
        }
        if (notePatchDto.IsBookmarked.TryGet(out var isBookmarked) && entity.IsBookmarked != isBookmarked)
        {
          entity.IsBookmarked = isBookmarked;
        }
        if (notePatchDto.IsDeleted.TryGet(out var isDeleted) && entity.IsDeleted != isDeleted)
        {
          entity.IsDeleted = isDeleted;
        }

        var updateResult = await context.SaveChangesAsync(cancellationToken);

        if (updateResult > 0)
        {
          entity.Modified = modified;
          var result = await context.SaveChangesAsync(cancellationToken);
          if (result > 0)
          {
            await transaction.CommitAsync(cancellationToken);
            return PersistenceMutationStatus.Applied;
          }
        }
        await transaction.RollbackAsync(cancellationToken);
        return PersistenceMutationStatus.Unchanged;
      }
      else
      {
        return PersistenceMutationStatus.NotFound;
      }
    }
    catch
    {
      await transaction.RollbackAsync(cancellationToken);
      return PersistenceMutationStatus.Failed;
    }
  }

  public async Task<PersistenceMutationStatus> UpdateNoteViewStateAsync(NoteViewStatePatchDto noteViewStatePatchDto, CancellationToken cancellationToken = default)
  {

    if (noteViewStatePatchDto.IsEmpty)
    {
      return PersistenceMutationStatus.Unchanged;
    }

    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);
    await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

    try
    {
      var updateResult = await context.EnqueueOperationAsync(
        operation: () => context.NoteViewStateEntities
          .Where(e => e.Id == noteViewStatePatchDto.Id.Value)
          .ExecuteUpdate(setters =>
          {
            if (noteViewStatePatchDto.ShowBackgroundImage.TryGet(out var showBackgroundImage))
            {
              setters.SetProperty(e => e.ShowBackgroundImage, showBackgroundImage);
            }
            if (noteViewStatePatchDto.BackgroundImageStretch.TryGet(out var backgroundImageStretch))
            {
              setters.SetProperty(e => e.BackgroundImageStretch, backgroundImageStretch);
            }
            if (noteViewStatePatchDto.BackgroundImageAlignment.TryGet(out var backgroundImageAlignment))
            {
              setters.SetProperty(e => e.BackgroundImageAlignment, (int)backgroundImageAlignment);
            }
            if (noteViewStatePatchDto.BackgroundImageOpacity.TryGet(out var backgroundImageOpacity))
            {
              setters.SetProperty(e => e.BackgroundImageOpacity, backgroundImageOpacity);
            }
            if (noteViewStatePatchDto.BackgroundImageBlur.TryGet(out var backgroundImageBlur))
            {
              setters.SetProperty(e => e.BackgroundImageBlur, backgroundImageBlur);
            }
            if (noteViewStatePatchDto.BackdropKind.TryGet(out var backdropKind))
            {
              setters.SetProperty(e => e.BackdropKind, (int)backdropKind);
            }
            if (noteViewStatePatchDto.BackdropTintOpacity.TryGet(out var backdropTintOpacity))
            {
              setters.SetProperty(e => e.BackdropTintOpacity, backdropTintOpacity);
            }
            if (noteViewStatePatchDto.BackdropLuminosityOpacity.TryGet(out var backdropLuminosityOpacity))
            {
              setters.SetProperty(e => e.BackdropLuminosityOpacity, backdropLuminosityOpacity);
            }
            if (noteViewStatePatchDto.ShowImagePanel.TryGet(out var showImagePanel))
            {
              setters.SetProperty(e => e.ShowImagePanel, showImagePanel);
            }
            if (noteViewStatePatchDto.ImagePanelHeight.TryGet(out var imagePanelHeight))
            {
              setters.SetProperty(e => e.ImagePanelHeight, imagePanelHeight);
            }
            if (noteViewStatePatchDto.Width.TryGet(out var width))
            {
              setters.SetProperty(e => e.Width, width);
            }
            if (noteViewStatePatchDto.Height.TryGet(out var height))
            {
              setters.SetProperty(e => e.Height, height);
            }
            if (noteViewStatePatchDto.PositionX.TryGet(out var positionX))
            {
              setters.SetProperty(e => e.PositionX, positionX);
            }
            if (noteViewStatePatchDto.PositionY.TryGet(out var positionY))
            {
              setters.SetProperty(e => e.PositionY, positionY);
            }
            if (noteViewStatePatchDto.IsTextEditorReadOnly.TryGet(out var isTextEditorReadOnly))
            {
              setters.SetProperty(e => e.IsTextEditorReadOnly, isTextEditorReadOnly);
            }
            if (noteViewStatePatchDto.IsWindowOpen.TryGet(out var isWindowOpen))
            {
              setters.SetProperty(e => e.IsWindowOpen, isWindowOpen);
            }
            if (noteViewStatePatchDto.IsAlwaysOnTop.TryGet(out var isAlwaysOnTop))
            {
              setters.SetProperty(e => e.IsAlwaysOnTop, isAlwaysOnTop);
            }
          }),
        defaultValue: 0,
        fallbackValue: 0,
        cancellationToken: cancellationToken);

      if (updateResult > 0)
      {
        await transaction.CommitAsync(cancellationToken);
        return PersistenceMutationStatus.Applied;
      }

      await transaction.RollbackAsync(cancellationToken);
      return PersistenceMutationStatus.NotFound;
    }
    catch
    {
      await transaction.RollbackAsync(cancellationToken);
      return PersistenceMutationStatus.Failed;
    }
  }

  public async Task<PersistenceMutationStatus> DeleteNoteAsync(NoteId noteId, DeleteMode deleteMode, CancellationToken cancellationToken = default)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);
    await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

    try
    {
      int deleteResult;
      switch (deleteMode)
      {
        case DeleteMode.MoveToTrash:
          deleteResult = await context.EnqueueOperationAsync(
            operation: () => context.NoteEntities
              .Where(e => e.Id == noteId.Value && !e.IsDeleted)
              .ExecuteUpdate(setters => setters.SetProperty(e => e.IsDeleted, true)),
            defaultValue: 0,
            fallbackValue: 0,
            cancellationToken: cancellationToken);
          break;
        case DeleteMode.Permanent:
          deleteResult = await context.EnqueueOperationAsync(
            operation: () => context.NoteEntities
              .Where(e => e.Id == noteId.Value)
              .ExecuteDelete(),
            defaultValue: 0,
            fallbackValue: 0,
            cancellationToken: cancellationToken);
          break;
        default:
          await transaction.RollbackAsync(cancellationToken);
          return PersistenceMutationStatus.Unchanged;
      }

      if (deleteResult > 0)
      {
        await transaction.CommitAsync(cancellationToken);
        return PersistenceMutationStatus.Applied;
      }

      await transaction.RollbackAsync(cancellationToken);
      return PersistenceMutationStatus.NotFound;
    }
    catch
    {
      await transaction.RollbackAsync(cancellationToken);
      return PersistenceMutationStatus.Unchanged;
    }
  }
}
