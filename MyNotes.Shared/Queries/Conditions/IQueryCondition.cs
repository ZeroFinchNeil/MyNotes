namespace MyNotes.Shared.Queries.Conditions;

internal interface IQueryCondition
{ }

internal interface IQueryCondition<out TCondition> : IQueryCondition where TCondition : notnull
{
  public TCondition Condition { get; }
}

internal interface IQueryCondition<TCondition, TSelf> : IQueryCondition<TCondition> where TCondition : notnull where TSelf : IQueryCondition<TCondition, TSelf>
{
  static abstract TSelf Create(TCondition condition);
}