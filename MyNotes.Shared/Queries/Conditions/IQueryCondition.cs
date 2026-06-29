using System;

namespace MyNotes.Shared.Queries.Conditions;

internal interface IQueryCondition
{ }

internal interface IQueryCondition<out TTarget, out TCondition> : IQueryCondition where TTarget : notnull where TCondition : Enum
{
  public TTarget Target { get; }

  public TCondition Condition { get; }
}

internal interface IQueryCondition<TTarget, TCondition, TSelf> : IQueryCondition<TTarget, TCondition> where TTarget : notnull where TCondition : Enum where TSelf : IQueryCondition<TTarget, TCondition, TSelf>
{
  static abstract TSelf Create(TTarget target, TCondition condition);
}