using System.Collections.Generic;

namespace MyNotes.Shared.Queries.Conditions;

internal interface IQueryConditionSet<out TKey, out TCondition> where TKey : notnull where TCondition : IQueryCondition
{
  public TKey Key { get; }

  public IEnumerable<TCondition> Conditions { get; }
}

internal interface IQueryConditionSet<TKey, TCondition, TSelf> : IQueryConditionSet<TKey, TCondition> where TKey : notnull where TCondition : IQueryCondition where TSelf : IQueryConditionSet<TKey, TCondition, TSelf>
{
  public static abstract TSelf Create(TKey key, IEnumerable<TCondition> conditions);
}