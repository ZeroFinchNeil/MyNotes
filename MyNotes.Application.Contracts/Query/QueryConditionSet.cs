using System.Collections.Generic;

using MyNotes.Application.Contracts.Query;

namespace MyNotes.Application.Contracts.Database.Query;

internal class QueryConditionSet<TKey, TCondition> where TKey : notnull where TCondition : IQueryCondition
{
  public required TKey Key { get; init; }

  public required IEnumerable<TCondition> Conditions { get; init; }
}