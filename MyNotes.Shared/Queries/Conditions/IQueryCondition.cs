namespace MyNotes.Shared.Queries.Conditions;

internal interface IQueryCondition
{ }

internal interface IQueryCondition<T> : IQueryCondition where T : notnull
{
  public T Condition { get; }
}