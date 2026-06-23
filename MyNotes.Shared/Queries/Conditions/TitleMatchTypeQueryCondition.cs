using MyNotes.Shared.Queries.Enums;

namespace MyNotes.Shared.Queries.Conditions;

internal class TitleMatchTypeQueryCondition : TitleQueryCondition, IQueryCondition<TitleMatchType>
{
  public required TitleMatchType Condition { get; init; }
}
