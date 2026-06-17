namespace MyNotes.Application.Contracts.Query;

internal interface IQueryCondition
{ }

internal interface IQueryCondition<T> : IQueryCondition where T : notnull
{
  public T Condition { get; }
}