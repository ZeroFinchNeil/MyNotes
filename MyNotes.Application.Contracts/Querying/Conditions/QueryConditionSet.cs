using System.Collections.Generic;

using MyNotes.Application.Contracts.Querying.Models;

namespace MyNotes.Application.Contracts.Querying.Conditions;

internal class QueryConditionSet<TCondition> : IQueryConditionSet<TCondition, QueryConditionSet<TCondition>> where TCondition : IQueryCondition
{
  public static QueryConditionSet<TCondition> Create(IReadOnlyList<TCondition> conditions, JoinOperator conditionOperator = JoinOperator.And) => new() { Conditions = conditions, ConditionOperator = conditionOperator };

  public required IReadOnlyList<TCondition> Conditions { get; init; }

  public required JoinOperator ConditionOperator { get; init; }
}