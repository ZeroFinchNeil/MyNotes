using MyNotes.Shared.Queries.Enums;

namespace MyNotes.Shared.Queries.Conditions;

internal class ComparisonOperatorQueryCondition : IQueryCondition<ComparisonOperator, ComparisonOperatorQueryCondition>
{
  public static ComparisonOperatorQueryCondition Create(ComparisonOperator condition) => new() { Condition = condition };

  public required ComparisonOperator Condition { get; init; }
}