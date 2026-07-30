using System.Collections.Generic;

using MyNotes.Application.Contracts.Querying.Models;

namespace MyNotes.Application.Contracts.Querying.Conditions;

internal interface IQueryConditionSet<out TCondition> where TCondition : IQueryCondition
{
  public IReadOnlyList<TCondition> Conditions { get; }

  public JoinOperator ConditionOperator { get; }
}

internal interface IQueryConditionSet<TCondition, TSelf> : IQueryConditionSet<TCondition> where TCondition : IQueryCondition where TSelf : IQueryConditionSet<TCondition, TSelf>
{
  public static abstract TSelf Create(IReadOnlyList<TCondition> conditions, JoinOperator conditionOperator = JoinOperator.And);
}