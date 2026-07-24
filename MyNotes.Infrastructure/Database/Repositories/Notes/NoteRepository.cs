using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using DotNext;

using Microsoft.EntityFrameworkCore;

using MyNotes.Application.Contracts.Database.Core;
using MyNotes.Application.Contracts.Database.Enums.Notes;
using MyNotes.Application.Contracts.Notes.Models.Common;
using MyNotes.Application.Contracts.Notes.Models.Creation;
using MyNotes.Application.Contracts.Notes.Models.Modification;
using MyNotes.Application.Contracts.Notes.Models.Queries;
using MyNotes.Application.Contracts.Notes.Models.Retrieval;
using MyNotes.Application.Contracts.Notes.Persistence;
using MyNotes.Common.Enums.Modes;
using MyNotes.Common.Expressions;
using MyNotes.Common.Querying;
using MyNotes.Common.Structures;
using MyNotes.Domain.ValueObjects;
using MyNotes.Infrastructure.Database.Core;
using MyNotes.Infrastructure.Database.Entities.Notes;
using MyNotes.Infrastructure.Mappers;
using MyNotes.Shared.Constants;
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

  public async Task<NoteBundleDbResponseDto> AddNoteAsync(CreateNoteBundleDbRequestDto createBundleDbRequestDto, IAppDbTransactionContext appDbTransactionContext, CancellationToken cancellationToken = default)
  {
    var context = appDbTransactionContext.DbContext as AppDbContext
      ?? throw new InvalidOperationException($"지원하지 않는 DbContext 타입입니다. Expected: {typeof(AppDbContext).FullName}, Actual: {appDbTransactionContext.DbContext.GetType().FullName}");

    try
    {
      NoteEntity noteEntity = NoteMappers.ToEntity(createBundleDbRequestDto.NoteDto);
      NoteViewStateEntity viewStateEntity = NoteMappers.ToEntity(createBundleDbRequestDto.ViewStateDto);

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

  public async Task<GetNoteFieldValuesDbResponseDto> GetNoteFieldValuesAsync(GetNoteFieldValuesDbRequestDto getFieldsDbRequestDto, CancellationToken cancellationToken = default)
  {
    await using AppDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken);

    var id = getFieldsDbRequestDto.Id;
    var getFields = getFieldsDbRequestDto.GetFields;
    return await context.NoteEntities
      .AsNoTracking()
      .Where(e => e.Id == id.Value)
      .Select(e => new GetNoteFieldValuesDbResponseDto()
      {
        Id = id,
        NavigationId = getFields.HasFlag(NoteGetFields.NavigationId)
          ? e.Navigation.HasValue
            ? new(NavigationId.Create(e.Navigation.Value)) : new(null)
          : Optional<NavigationId?>.None,
        Created = getFields.HasFlag(NoteGetFields.Created) ? new(e.Created) : Optional<DateTimeOffset>.None,
        Modified = getFields.HasFlag(NoteGetFields.Modified) ? new(e.Modified) : Optional<DateTimeOffset>.None,
        Title = getFields.HasFlag(NoteGetFields.Title) ? new(e.Title) : Optional<string>.None,
        Body = getFields.HasFlag(NoteGetFields.Body) ? new(e.Body) : Optional<string>.None,
        BackgroundColor = getFields.HasFlag(NoteGetFields.BackgroundColor) ? new(e.BackgroundColor) : Optional<string>.None,
        IsBookmarked = getFields.HasFlag(NoteGetFields.IsBookmarked) ? new(e.IsBookmarked) : Optional<bool>.None,
        IsDeleted = getFields.HasFlag(NoteGetFields.IsDeleted) ? new(e.IsDeleted) : Optional<bool>.None,
      })
      .FirstOrDefaultAsync(cancellationToken)
      ?? new GetNoteFieldValuesDbResponseDto()
      {
        Id = id
      };
  }

  public async Task<IReadOnlyList<NoteBundleDbResponseDto>> GetNotesByParentAsync(NavigationId navigationId, bool includeDeleted = false, CancellationToken cancellationToken = default)
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

  public async Task<IReadOnlyList<NoteBundleDbResponseDto>> FindNotesAsync(FindNotesDbQuery findDbQuery, CancellationToken cancellationToken = default)
  {
    //todo: DB에서 쿼리 조건에 따른 노트 가져오기
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);

    var expressions = MakeExpressions(findDbQuery);
    if (expressions.Count == 0)
    {

    }

    var expression = findDbQuery.AggregationMode switch
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

  public async Task<UpdateNoteDbResponseDto> UpdateNoteAsync(UpdateNoteDbRequestDto updateDbRequestDto, bool updateIfChanged = true, CancellationToken cancellationToken = default)
  {
    var id = updateDbRequestDto.Id;

    UpdateNoteDbResponseDto responseDto = new() { Id = id };
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);

    if (!updateDbRequestDto.IsEmpty
        && await context.NoteEntities.Where(e => e.Id == id.Value).SingleOrDefaultAsync(cancellationToken) is NoteEntity entity)
    {
      if (updateDbRequestDto.NavigationId.TryGetSpecifiedValue(out var navigationId) && entity.Navigation != navigationId.Value)
      {
        entity.Navigation = navigationId.Value;
        responseDto = responseDto with { NavigationId = navigationId };
      }
      if (updateDbRequestDto.Title.TryGet(out var title) && entity.Title != title)
      {
        entity.Title = title;
        responseDto = responseDto with { Title = title };
      }
      if (updateDbRequestDto.Body.TryGet(out var body) && entity.Body != body)
      {
        entity.Body = body;
        responseDto = responseDto with { Body = body };
      }
      if (updateDbRequestDto.BodyImagePaths.TryGet(out var bodyImagePaths) && JsonSerializer.Serialize(bodyImagePaths, AppJson.JsonSerializerOptions) is { } bodyImagePathsJson)
      {
        entity.BodyImagePaths = bodyImagePathsJson;
        responseDto = responseDto with { BodyImagePaths = new(bodyImagePaths) };
      }
      if (updateDbRequestDto.BackgroundColor.TryGet(out var backgroundColor) && entity.BackgroundColor != backgroundColor)
      {
        entity.BackgroundColor = backgroundColor;
        responseDto = responseDto with { BackgroundColor = backgroundColor };
      }
      if (updateDbRequestDto.BackgroundImagePath.TryGet(out var backgroundImagePath) && entity.BackgroundImagePath != backgroundImagePath)
      {
        entity.BackgroundImagePath = backgroundImagePath;
        responseDto = responseDto with { BackgroundImagePath = backgroundImagePath };
      }
      if (updateDbRequestDto.IsBookmarked.TryGet(out var isBookmarked) && entity.IsBookmarked != isBookmarked)
      {
        entity.IsBookmarked = isBookmarked;
        responseDto = responseDto with { IsBookmarked = isBookmarked };
      }
      if (updateDbRequestDto.IsDeleted.TryGet(out var isDeleted) && entity.IsDeleted != isDeleted)
      {
        entity.IsDeleted = isDeleted;
        responseDto = responseDto with { IsDeleted = isDeleted };
      }

      var modified = updateDbRequestDto.Modified;
      if (!responseDto.IsEmpty && entity.Modified < modified)
      {
        entity.Modified = modified;
        responseDto = responseDto with { Modified = modified };
      }
      var result = await context.SaveChangesAsync(cancellationToken);
    }

    return responseDto;
  }

  public async Task<UpdateNoteViewStateDbResponseDto> UpdateNoteViewStateAsync(UpdateNoteViewStateDbRequestDto updateDbRequestDto, bool updateIfChanged = true, CancellationToken cancellationToken = default)
  {
    var id = updateDbRequestDto.Id;
    UpdateNoteViewStateDbResponseDto responseDto = new() { Id = id };

    if (updateDbRequestDto.IsEmpty)
    {
      return responseDto;
    }

    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);
    var updateResult = await context.EnqueueOperationAsync(
      operation: () => context.NoteViewStateEntities
        .Where(e => e.Id == id.Value)
        .ExecuteUpdate(setters =>
        {
          if (updateDbRequestDto.ShowBackgroundImage.TryGet(out var showBackgroundImage))
          {
            setters.SetProperty(e => e.ShowBackgroundImage, showBackgroundImage);
          }
          if (updateDbRequestDto.BackgroundImageStretch.TryGet(out var backgroundImageStretch))
          {
            setters.SetProperty(e => e.BackgroundImageStretch, backgroundImageStretch);
          }
          if (updateDbRequestDto.BackgroundImageAlignment.TryGet(out var backgroundImageAlignment))
          {
            setters.SetProperty(e => e.BackgroundImageAlignment, backgroundImageAlignment);
          }
          if (updateDbRequestDto.BackgroundImageOpacity.TryGet(out var backgroundImageOpacity))
          {
            setters.SetProperty(e => e.BackgroundImageOpacity, backgroundImageOpacity);
          }
          if (updateDbRequestDto.BackgroundImageBlur.TryGet(out var backgroundImageBlur))
          {
            setters.SetProperty(e => e.BackgroundImageBlur, backgroundImageBlur);
          }
          if (updateDbRequestDto.BackdropKind.TryGet(out var backdropKind))
          {
            setters.SetProperty(e => e.BackdropKind, backdropKind);
          }
          if (updateDbRequestDto.BackdropTintOpacity.TryGet(out var backdropTintOpacity))
          {
            setters.SetProperty(e => e.BackdropTintOpacity, backdropTintOpacity);
          }
          if (updateDbRequestDto.BackdropLuminosityOpacity.TryGet(out var backdropLuminosityOpacity))
          {
            setters.SetProperty(e => e.BackdropLuminosityOpacity, backdropLuminosityOpacity);
          }
          if (updateDbRequestDto.ShowImagePanel.TryGet(out var showImagePanel))
          {
            setters.SetProperty(e => e.ShowImagePanel, showImagePanel);
          }
          if (updateDbRequestDto.ImagePanelHeight.TryGet(out var imagePanelHeight))
          {
            setters.SetProperty(e => e.ImagePanelHeight, imagePanelHeight);
          }
          if (updateDbRequestDto.Width.TryGet(out var width))
          {
            setters.SetProperty(e => e.Width, width);
          }
          if (updateDbRequestDto.Height.TryGet(out var height))
          {
            setters.SetProperty(e => e.Height, height);
          }
          if (updateDbRequestDto.PositionX.TryGet(out var positionX))
          {
            setters.SetProperty(e => e.PositionX, positionX);
          }
          if (updateDbRequestDto.PositionY.TryGet(out var positionY))
          {
            setters.SetProperty(e => e.PositionY, positionY);
          }
          if (updateDbRequestDto.IsTextEditorReadOnly.TryGet(out var isTextEditorReadOnly))
          {
            setters.SetProperty(e => e.IsTextEditorReadOnly, isTextEditorReadOnly);
          }
          if (updateDbRequestDto.IsWindowOpen.TryGet(out var isWindowOpen))
          {
            setters.SetProperty(e => e.IsWindowOpen, isWindowOpen);
          }
          if (updateDbRequestDto.IsAlwaysOnTop.TryGet(out var isAlwaysOnTop))
          {
            setters.SetProperty(e => e.IsAlwaysOnTop, isAlwaysOnTop);
          }
        }),
      defaultValue: 0,
      fallbackValue: 0,
      cancellationToken: cancellationToken);

    return responseDto;
  }

  public async Task<bool> DeleteNoteAsync(DeleteNoteDbRequestDto deleteDbRequestDto, CancellationToken cancellationToken = default)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);
    var id = deleteDbRequestDto.Id;

    switch (deleteDbRequestDto.DeleteMode)
    {
      case DeleteMode.MoveToTrash:
        var updateResult = await context.EnqueueOperationAsync(
          operation: () => context.NoteEntities
          .Where(e => e.Id == id.Value && !e.IsDeleted)
          .ExecuteUpdate(setters => setters.SetProperty(e => e.IsDeleted, true)),
          defaultValue: 0,
          fallbackValue: 0,
          cancellationToken: cancellationToken);
        return updateResult > 0;
      case DeleteMode.Permanent:
        var deleteResult = await context.EnqueueOperationAsync(
          operation: () => context.NoteEntities
          .Where(e => e.Id == id.Value)
          .ExecuteDelete(),
          defaultValue: 0,
          fallbackValue: 0,
          cancellationToken: cancellationToken);
        return deleteResult > 0;
    }

    return false;
  }
}
