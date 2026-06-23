using MyNotes.Shared.Queries.Enums;

namespace MyNotes.Shared.Queries.Conditions;

internal class ModifiedComparisonQueryCondition : ModifiedQueryCondition, IQueryCondition<ComparisonOperator>
{
  public required ComparisonOperator Condition { get; init; }
}