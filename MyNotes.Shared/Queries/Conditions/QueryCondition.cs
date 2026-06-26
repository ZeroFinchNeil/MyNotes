namespace MyNotes.Shared.Queries.Conditions;

internal class QueryCondition<TCondition, TSelf> : IQueryCondition<TCondition, TSelf> where TCondition : notnull where TSelf : QueryCondition<TCondition, TSelf>, new()
{
  public static TSelf Create(TCondition condition) => new() { Condition = condition };

  public required TCondition Condition { get; init; }
}