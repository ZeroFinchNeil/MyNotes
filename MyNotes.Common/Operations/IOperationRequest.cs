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
  public TaskCompletionSource<T> TaskCompletionSource { get; }
  public Func<T> Operation { get; }
  public T FallbackValue { get; }

  Task IOperationRequest.Task => TaskCompletionSource.Task;
}
