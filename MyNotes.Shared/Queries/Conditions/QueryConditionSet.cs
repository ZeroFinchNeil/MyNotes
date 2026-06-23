using System.Collections.Generic;

namespace MyNotes.Shared.Queries.Conditions;

internal class QueryConditionSet<TKey, TCondition> where TKey : notnull where TCondition : IQueryCondition
{
  public required TKey Key { get; init; }

  public required IEnumerable<TCondition> Conditions { get; init; }
}