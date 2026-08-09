using System;
using System.Threading.Tasks;

namespace MyNotes.Common.Operations;

internal abstract class OperationRequest : IOperationRequest
{
  protected TaskCompletionSource TaskCompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

  public Task Task => TaskCompletionSource.Task;

  public abstract void Execute();
}

internal abstract class OperationRequest<T>(Func<T> operation, T fallbackValue) : IOperationRequest<T>
{
  protected TaskCompletionSource<T> TaskCompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

  public Func<T> Operation { get; } = operation;
  public T FallbackValue { get; } = fallbackValue;

  public Task<T> Result => TaskCompletionSource.Task;

  public abstract void Execute();
}