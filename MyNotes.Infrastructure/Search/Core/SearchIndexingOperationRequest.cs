using System;
using System.Threading;
using System.Threading.Tasks;

using MyNotes.Common.Operations;
using MyNotes.Debugging;

namespace MyNotes.Infrastructure.Search.Core;

internal class SearchIndexingOperationRequest<T>(Func<T> operation, T fallbackValue = default!) : OperationRequest<T>(operation, fallbackValue)
{
  public override void Execute()
  {
    try
    {
      var result = Operation.Invoke();
      ConsoleHelper.WriteLine(true, "{0}: {1}", "SearchIndexing Operation invoked", result);
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

  public Task<T> WaitAsync(CancellationToken cancellationToken = default) => TaskCompletionSource.Task.WaitAsync(cancellationToken);
}

internal class SearchIndexingOperationRequest(Action operation) : OperationRequest
{
  public Action Operation { get; } = operation;

  public override void Execute()
  {
    try
    {
      Operation.Invoke();
      ConsoleHelper.WriteLine(true, "SearchIndexing Operation invoked");
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

  public Task WaitAsync(CancellationToken cancellationToken = default) => TaskCompletionSource.Task.WaitAsync(cancellationToken);
}