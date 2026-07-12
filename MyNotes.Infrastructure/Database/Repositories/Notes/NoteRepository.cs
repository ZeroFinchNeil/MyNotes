using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MyNotes.Application.Contracts.Database.Core;
using MyNotes.Application.Contracts.Database.Dtos.Notes.Common;
using MyNotes.Application.Contracts.Database.Dtos.Notes.Creation;
using MyNotes.Application.Contracts.Database.Dtos.Notes.Modification;
using MyNotes.Application.Contracts.Database.Dtos.Notes.Queries;
using MyNotes.Application.Contracts.Database.Enums.Notes;
using MyNotes.Application.Contracts.Database.Repositories.Notes;
using MyNotes.Common.Expressions;
using MyNotes.Common.Querying;
using MyNotes.Domain.ValueObjects;
using MyNotes.Infrastructure.Database.Core;
using MyNotes.Infrastructure.Database.Entities.Notes;
using MyNotes.Infrastructure.Mappers;
using MyNotes.Shared.Queries.Conditions;

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

  public async Task<NoteBundleDbResponseDto> AddNoteAsync(CreateNoteBundleDbRequestDto bundleDbDto, IAppDbTransactionContext appDbTransactionContext, CancellationToken cancellationToken = default)
  {
    var context = appDbTransactionContext.DbContext as AppDbContext
      ?? throw new InvalidOperationException($"지원하지 않는 DbContext 타입입니다. Expected: {typeof(AppDbContext).FullName}, Actual: {appDbTransactionContext.DbContext.GetType().FullName}");

    try
    {
      NoteEntity noteEntity = NoteMappers.ToEntity(bundleDbDto.NoteDto);
      NoteViewStateEntity viewStateEntity = NoteMappers.ToEntity(bundleDbDto.ViewStateDto);

      await context.NoteEntities.AddAsync(noteEntity, cancellationToken);
      await context.NoteViewStateEntities.AddAsync(viewStateEntity, cancellationToken);

      return NoteMappers.ToDto(noteEntity, viewStateEntity);
    }
    catch
    {
      throw;
    }
  }

  public async Task<NoteBundleDbResponseDto?> GetNoteByIdAsync(NoteId noteId, CancellationToken cancellationToken = default)
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

  public async Task<NoteViewStateDbResponseDto?> GetNoteViewStateByIdAsync(NoteId noteId, CancellationToken cancellationToken = default)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);
    return await context.NoteViewStateEntities
      .AsNoTracking()
      .Where(e => e.Id == noteId.Value)
      .Select(e => NoteMappers.ToDto(e))
      .FirstOrDefaultAsync(cancellationToken);
  }

  public async Task<IReadOnlyList<NoteBundleDbResponseDto>> GetNotesByParentAsync(NavigationId parentId, bool includeDeleted = false, CancellationToken cancellationToken = default)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);
    Expression<Func<NoteEntity, bool>> predicate = includeDeleted
      ? e => e.Parent == parentId.Value
      : e => e.Parent == parentId.Value && e.IsDeleted == false;
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

  public async Task<IReadOnlyList<NoteBundleDbResponseDto>> FindNotesAsync(FindNotesDbQuery findNotesDbQuery, CancellationToken cancellationToken = default)
  {
    //todo: DB에서 쿼리 조건에 따른 노트 가져오기
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);

    var expressions = MakeExpressions(findNotesDbQuery);
    if (expressions.Count == 0)
    {

    }

    var expression = findNotesDbQuery.AggregationMode switch
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

  public static IReadOnlyList<Expression<Func<NoteEntity, bool>>> MakeExpressions(FindNotesDbQuery query)
  {
    var noteFindFields = query.NoteFindFields;

    if (noteFindFields == NoteFindFields.None)
    {
      throw new ArgumentException("", nameof(query));
    }

    List<Expression<Func<NoteEntity, bool>>> expressions = new();
    if (noteFindFields.HasFlag(NoteFindFields.NoteIdCondition))
    {
      var noteIdCondition = query.NoteIdCondition ?? throw new ArgumentException("", nameof(query));
      expressions.Add(CreateExpression(noteIdCondition, e => e.Id));
    }
    if (noteFindFields.HasFlag(NoteFindFields.ParentIdCondition))
    {
      var parentIdCondition = query.ParentIdCondition ?? throw new ArgumentException("", nameof(query));
      expressions.Add(CreateExpression(parentIdCondition, e => e.Id));
    }
    if (noteFindFields.HasFlag(NoteFindFields.TitleConditions))
    {

    }
    if (noteFindFields.HasFlag(NoteFindFields.CreatedConditions))
    {
      var createdConditions = query.CreatedConditions ?? throw new ArgumentException("", nameof(query));
      expressions.Add(CombineExpressions(createdConditions, e => e.Created));
    }
    if (noteFindFields.HasFlag(NoteFindFields.ModifiedConditions))
    {
      var modifiedConditions = query.ModifiedConditions ?? throw new ArgumentException("", nameof(query));
      expressions.Add(CombineExpressions(modifiedConditions, e => e.Modified));
    }
    if (noteFindFields.HasFlag(NoteFindFields.BackgroundColorConditions))
    {
    }
    if (noteFindFields.HasFlag(NoteFindFields.BookmarkedCondition))
    {
      var bookmarkedCondition = query.BookmarkedCondition ?? throw new ArgumentException("", nameof(query));
      expressions.Add(CreateExpression(bookmarkedCondition, e => e.IsBookmarked));
    }
    if (noteFindFields.HasFlag(NoteFindFields.DeletedCondition))
    {
      var deletedCondition = query.DeletedCondition ?? throw new ArgumentException("", nameof(query));
      expressions.Add(CreateExpression(deletedCondition, e => e.IsDeleted));
    }

    return expressions;
  }

  public Task<bool> UpdateNoteAsync(UpdateNoteDbRequestDto updateNoteDbDto, bool updateIfChanged = true, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }

  public Task<bool> UpdateNoteViewStateAsync(UpdateNoteViewStateDbRequestDto updateNoteViewStateDbDto, bool updateIfChanged = true, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }

  public Task<bool> DeleteNoteAsync(NoteId noteId, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }
}
