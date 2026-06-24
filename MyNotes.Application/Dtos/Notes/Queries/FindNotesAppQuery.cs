using MyNotes.Domain.ValueObjects;
using MyNotes.Shared.Queries.Conditions;

namespace MyNotes.Application.Dtos.Notes.Queries;

internal sealed record FindNotesAppQuery
{
  public NoteId? NoteId { get; init; }

  public NavigationId? NavigationId { get; init; }

  public QueryConditionSet<string, TitleQueryCondition>? TitleConditions { get; init; }

  public QueryConditionSet<DateTimeOffset, CreatedQueryCondition>? CreatedConditions { get; init; }

  public QueryConditionSet<DateTimeOffset, ModifiedQueryCondition>? ModifiedConditions { get; init; }

  public bool? IsBookmarked { get; init; }

  public bool? IsDeleted { get; init; }
}