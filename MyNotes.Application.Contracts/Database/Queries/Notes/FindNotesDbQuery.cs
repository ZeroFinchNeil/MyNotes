using System;

using MyNotes.Application.Contracts.Database.Queries.Notes.Conditions;
using MyNotes.Application.Contracts.Queries;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Queries.Notes;

internal sealed record FindNotesDbQuery
{
  public NoteId? NoteId { get; init; }

  public NavigationId? NavigationId { get; init; }

  public QueryConditionSet<string, TitleQueryCondition>? TitleConditions { get; init; }

  public QueryConditionSet<DateTimeOffset, CreatedQueryCondition>? CreatedConditions { get; init; }

  public QueryConditionSet<DateTimeOffset, ModifiedQueryCondition>? ModifiedConditions { get; init; }
}