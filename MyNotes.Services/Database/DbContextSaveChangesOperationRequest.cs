using System;
using System.Threading.Tasks;

using MyNotes.Common.Operations;

namespace MyNotes.Services.Database;

internal sealed class DbContextSaveChangesOperationRequest(Func<Task<int>> operation, int fallbackValue = 0) : IAsyncOperationRequest<int>
{
  public TaskCompletionSource<int> TaskCompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
  public Func<Task<int>> Operation { get; } = operation;
  public int FallbackValue { get; } = fallbackValue;

  public async Task ExecuteAsync()
  {
    try
    {
      var result = await Operation.Invoke();
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
        TaskCompletionSource.SetResult(FallbackValue);
    }
  }
}