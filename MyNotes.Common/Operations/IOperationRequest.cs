using System;
using System.Threading.Tasks;

namespace MyNotes.Common.Operations;

public interface IOperationRequest
{
  public Task Task { get; }
  public void Execute();
}

public interface IOperationRequest<T> : IOperationRequest
{
  public Func<T> Operation { get; }
  public T FallbackValue { get; }

  public Task<T> Result { get; }

  Task IOperationRequest.Task => Result;
}
