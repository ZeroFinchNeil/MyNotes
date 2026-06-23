using System;

using MyNotes.Application.Contracts.Query;
using MyNotes.Domain.ValueObjects;
using MyNotes.Shared.Queries.Conditions;

namespace MyNotes.Application.Contracts.Database.Dtos.Notes.Queries;

internal sealed record FindNotesDbQuery
{
  public NoteId? NoteId { get; init; }

  public NavigationId? NavigationId { get; init; }

  public QueryConditionSet<string, TitleQueryCondition>? TitleConditions { get; init; }

  public QueryConditionSet<DateTimeOffset, CreatedQueryCondition>? CreatedConditions { get; init; }

  public QueryConditionSet<DateTimeOffset, ModifiedQueryCondition>? ModifiedConditions { get; init; }
}