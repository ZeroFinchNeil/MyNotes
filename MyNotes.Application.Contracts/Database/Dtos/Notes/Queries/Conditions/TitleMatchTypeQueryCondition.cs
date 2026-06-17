using MyNotes.Application.Contracts.Database.Dtos.Notes.Queries.Enums;
using MyNotes.Application.Contracts.Query;

namespace MyNotes.Application.Contracts.Database.Dtos.Notes.Queries.Conditions;

internal class TitleMatchTypeQueryCondition : TitleQueryCondition, IQueryCondition<TitleMatchType>
{
  public required TitleMatchType Condition { get; init; }
}
