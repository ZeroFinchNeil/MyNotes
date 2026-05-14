using System;
using System.Threading.Tasks;

using MyNotes.Common.Operations;

namespace MyNotes.Infrastructure.Search.Core;

internal class SearchIndexingOperationRequest<T>(Func<T> operation, T fallbackValue = default!) : IOperationRequest<T>
{
  public TaskCompletionSource<T> TaskCompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
  public Func<T> Operation { get; } = operation;
  public T FallbackValue { get; } = fallbackValue;

  public void Execute()
  {
    try
    {
      var result = Operation.Invoke();
      TaskCompletionSource.TrySetResult(result);
    }
    catch (OperationCanceledException)
    {
      TaskCompletionSource.TrySetCanceled();
    }
    catch (Exception ex)
    {
      TaskCompletionSource.TrySetException(ex);
    }
    finally
    {
      if (!TaskCompletionSource.Task.IsCompleted)
      {
        TaskCompletionSource.SetResult(FallbackValue);
      }
    }
  }
}

internal class SearchIndexingOperationRequest(Action operation) : IOperationRequest
{
  public TaskCompletionSource TaskCompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
  public Action Operation { get; } = operation;

  public Task Task => TaskCompletionSource.Task;

  public void Execute()
  {
    try
    {
      Operation.Invoke();
      TaskCompletionSource.TrySetResult();
    }
    catch (OperationCanceledException)
    {
      TaskCompletionSource.TrySetCanceled();
    }
    catch (Exception ex)
    {
      TaskCompletionSource.TrySetException(ex);
    }
    finally
    {
      if (!TaskCompletionSource.Task.IsCompleted)
      {
        TaskCompletionSource.SetResult();
      }
    }
  }
}