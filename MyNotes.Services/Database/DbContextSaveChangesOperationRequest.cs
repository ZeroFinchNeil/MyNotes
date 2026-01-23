using System;
using System.Threading.Tasks;

using MyNotes.Common.Operations;

namespace MyNotes.Services.Database;

internal sealed class DbContextSaveChangesOperationRequest(Func<int> operation, int fallbackValue = 0) : IOperationRequest<int>
{
  public TaskCompletionSource<int> TaskCompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
  public Func<int> Operation { get; } = operation;
  public int FallbackValue { get; } = fallbackValue;

  public void Execute()
  {
    try
    {
      var result = Operation.Invoke();
      var r = TaskCompletionSource.TrySetResult(result);
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