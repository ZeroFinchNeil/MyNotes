using MyNotes.Domain.ValueObjects;
using MyNotes.Shared.Queries.Conditions;

namespace MyNotes.Application.Dtos.Notes.Queries;

internal sealed record SearchNotesAppQuery
{
  public NoteId? NoteId { get; init; }

  public QueryConditionSet<string, TitleQueryCondition>? TitleConditions { get; init; }

  public QueryConditionSet<string, TitleQueryCondition>? BodyConditions { get; init; }
}