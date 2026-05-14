namespace MyNotes.Application.Contracts.Queries;

internal interface IQueryCondition
{ }

internal interface IQueryCondition<T> : IQueryCondition where T : notnull
{
  public T Condition { get; }
}