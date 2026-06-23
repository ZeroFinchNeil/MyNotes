using MyNotes.Shared.Queries.Enums;

namespace MyNotes.Shared.Queries.Conditions;

internal class CreatedComparisonQueryCondition : CreatedQueryCondition, IQueryCondition<ComparisonOperator>
{
  public required ComparisonOperator Condition { get; init; }
}