using MyNotes.Shared.Queries.Enums;

namespace MyNotes.Shared.Queries.Conditions;

internal class StringMatchTypeQueryCondition : IQueryCondition<StringMatchType, StringMatchTypeQueryCondition>
{
  public static StringMatchTypeQueryCondition Create(StringMatchType condition) => new() { Condition = condition };

  public required StringMatchType Condition { get; init; }
}