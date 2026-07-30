using System;

namespace MyNotes.Application.Contracts.Querying.Conditions;

internal class QueryCondition<TTarget, TCondition> : IQueryCondition<TTarget, TCondition, QueryCondition<TTarget, TCondition>> where TTarget : notnull where TCondition : Enum
{
  public static QueryCondition<TTarget, TCondition> Create(TTarget target, TCondition condition) => new() { Target = target, Condition = condition };

  public required TTarget Target { get; init; }

  public required TCondition Condition { get; init; }
}