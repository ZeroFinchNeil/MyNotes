using MyNotes.Application.Contracts.Database.Queries.Notes.Enums;
using MyNotes.Application.Contracts.Queries;

namespace MyNotes.Application.Contracts.Database.Queries.Notes.Conditions;

internal class TitleMatchTypeQueryCondition : TitleQueryCondition, IQueryCondition<TitleMatchType>
{
  public required TitleMatchType Condition { get; init; }
}
