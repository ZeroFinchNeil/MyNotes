using MyNotes.Domain.ValueObjects;
using MyNotes.Shared.Queries.Conditions;

namespace MyNotes.Application.Dtos.Notes.Queries;

internal sealed record SearchNotesAppQuery
{
  public NoteId? NoteId { get; init; }

  public QueryConditionSet<StringQueryCondition>? TitleConditions { get; init; }

  public QueryConditionSet<StringQueryCondition>? BodyConditions { get; init; }
}