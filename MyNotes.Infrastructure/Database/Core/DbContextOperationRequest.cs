using System;
using System.Threading.Tasks;

using MyNotes.Common.Operations;
using MyNotes.Debugging;

namespace MyNotes.Infrastructure.Database.Core;

internal sealed class DbContextOperationRequest<T>(Func<T> operation, T fallbackValue) : OperationRequest<T>(operation, fallbackValue)
{
  public override void Execute()
  {
    try
    {
      var result = Operation.Invoke();
      ConsoleHelper.WriteLine(true, "{0}: {1}", "DbContext Operation invoked", result);
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
      {
        TaskCompletionSource.SetResult(FallbackValue);
      }
    }
  }
}