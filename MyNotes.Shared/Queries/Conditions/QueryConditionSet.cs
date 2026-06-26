using System.Collections.Generic;

namespace MyNotes.Shared.Queries.Conditions;

internal class QueryConditionSet<TKey, TCondition> : IQueryConditionSet<TKey, TCondition, QueryConditionSet<TKey, TCondition>> where TKey : notnull where TCondition : IQueryCondition
{
  public static QueryConditionSet<TKey, TCondition> Create(TKey key, IEnumerable<TCondition> conditions) => new() { Key = key, Conditions = conditions };

  public required TKey Key { get; init; }

  public required IEnumerable<TCondition> Conditions { get; init; }
}