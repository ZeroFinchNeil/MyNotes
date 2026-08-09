using System;
using System.Threading.Tasks;

namespace MyNotes.Common.Operations;

internal abstract class AsyncOperationRequest : IAsyncOperationRequest
{
  protected TaskCompletionSource TaskCompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

  public Task Task => TaskCompletionSource.Task;

  public abstract Task ExecuteAsync();
}

internal abstract class AsyncOperationRequest<T>(Func<Task<T>> operation, T fallbackValue) : IAsyncOperationRequest<T>
{
  protected TaskCompletionSource<T> TaskCompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

  public Func<Task<T>> Operation { get; } = operation;
  public T FallbackValue { get; } = fallbackValue;
  public Task<T> Result => TaskCompletionSource.Task;

  public abstract Task ExecuteAsync();
}