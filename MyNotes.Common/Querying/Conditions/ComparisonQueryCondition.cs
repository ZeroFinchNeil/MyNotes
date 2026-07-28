namespace MyNotes.Common.Querying.Conditions;

internal class ComparisonQueryCondition<TTarget> : IQueryCondition<TTarget, ComparisonOperator, ComparisonQueryCondition<TTarget>> where TTarget : notnull
{
  public static ComparisonQueryCondition<TTarget> Create(TTarget target, ComparisonOperator condition) => new() { Target = target, Condition = condition };

  public required TTarget Target { get; init; }

  public required ComparisonOperator Condition { get; init; }
}