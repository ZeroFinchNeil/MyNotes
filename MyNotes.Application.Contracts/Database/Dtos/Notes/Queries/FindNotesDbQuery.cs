using System;

using MyNotes.Domain.ValueObjects;
using MyNotes.Shared.Queries.Conditions;

namespace MyNotes.Application.Contracts.Database.Dtos.Notes.Queries;

internal sealed record FindNotesDbQuery
{
  public NoteId? NoteId { get; init; }

  public NavigationId? NavigationId { get; init; }

  public QueryConditionSet<DateTimeOffset, ComparisonOperatorQueryCondition>? CreatedConditions { get; init; }

  public QueryConditionSet<DateTimeOffset, ComparisonOperatorQueryCondition>? ModifiedConditions { get; init; }

  public QueryConditionSet<string, StringMatchTypeQueryCondition>? TitleConditions { get; init; }

  public QueryConditionSet<string, StringMatchTypeQueryCondition>? BodyConditions { get; init; }

  public string? BackgroundColor { get; init; }

  public bool? IsBookmarked { get; init; }

  public bool? IsDeleted { get; init; }
}