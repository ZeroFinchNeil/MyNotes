using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MyNotes.Services.Database;

internal sealed class AppDbContextTaskDispatcher : IDisposable
{
  private readonly Channel<DbSaveChangesOperation> SaveChangesChannel = Channel.CreateUnbounded<DbSaveChangesOperation>(new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false });

  public AppDbContextTaskDispatcher()
  {
    _ = RunWorker();
  }

  public async Task<int> EnqueueAsync(Task<int> saveChanges, CancellationToken cancellationToken = default)
  {
    DbSaveChangesOperation request = new(() => saveChanges);
    await SaveChangesChannel.Writer.WriteAsync(request, cancellationToken);
    return await request.TaskCompletionSource.Task;
  }

  private Task RunWorker() => Task.Run(async () =>
  {
    await foreach (DbSaveChangesOperation request in SaveChangesChannel.Reader.ReadAllAsync())
    {
      try
      {
        var result = await request.Operation.Invoke();
        request.TaskCompletionSource.TrySetResult(result);
      }
      catch (OperationCanceledException)
      {
        request.TaskCompletionSource.TrySetCanceled();
      }
      catch (Exception ex)
      {
        request.TaskCompletionSource.TrySetException(ex);
      }
      finally
      {
        if (!request.TaskCompletionSource.Task.IsCompleted)
          request.TaskCompletionSource.SetResult(0);
      }
    }
  });

  private bool _disposed;

  private void Dispose(bool disposing)
  {
    if (!_disposed)
    {
      if (disposing)
      {
        SaveChangesChannel.Writer.Complete();
      }
      _disposed = true;
    }
  }


  public void Dispose()
  {
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }
}

internal class DbSaveChangesOperation(Func<Task<int>> saveChangesOperation)
{
  public TaskCompletionSource<int> TaskCompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
  public Func<Task<int>> Operation { get; } = saveChangesOperation;
}