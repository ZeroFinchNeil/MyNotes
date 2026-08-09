using System;
using System.Threading.Tasks;

using MyNotes.Common.Operations;

namespace MyNotes.Infrastructure.Database.Core;

internal sealed class DbContextOperationRequest<T>(Func<T> operation, T fallbackValue) : OperationRequest<T>(operation, fallbackValue)
{
  public override void Execute()
  {
    try
    {
      var result = Operation.Invoke();
      Console.WriteLine("{0}: {1}", "Operation invoked", result);
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