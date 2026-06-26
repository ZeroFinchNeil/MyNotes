using MyNotes.Domain.ValueObjects;
using MyNotes.Shared.Queries.Conditions;

namespace MyNotes.Application.Dtos.Notes.Queries;

internal sealed record FindNotesAppQuery
{
  public NoteId? NoteId { get; init; }

  public NavigationId? NavigationId { get; init; }

  public QueryConditionSet<string, StringMatchTypeQueryCondition>? TitleConditions { get; init; }

  public QueryConditionSet<DateTimeOffset, ComparisonOperatorQueryCondition>? CreatedConditions { get; init; }

  public QueryConditionSet<DateTimeOffset, ComparisonOperatorQueryCondition>? ModifiedConditions { get; init; }

  public bool? IsBookmarked { get; init; }

  public bool? IsDeleted { get; init; }
}