using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using MyNotes.Common.Operations;

namespace MyNotes.Services.Database;

internal sealed class AppDbContextTaskDispatcher : IDisposable
{
  private readonly Channel<IAsyncOperationRequest> DbContextChannel = Channel.CreateUnbounded<IAsyncOperationRequest>(new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false });

  public AppDbContextTaskDispatcher()
  {
    _ = RunWorker();
  }

  public async Task<int> EnqueueSaveChangesAsync(Func<Task<int>> saveChanges, CancellationToken cancellationToken = default)
  {
    DbContextSaveChangesOperationRequest request = new(saveChanges);
    await DbContextChannel.Writer.WriteAsync(request, cancellationToken);
    return await request.TaskCompletionSource.Task.WaitAsync(cancellationToken);
  }

  private Task RunWorker() => Task.Run(async () =>
  {
    await foreach (IAsyncOperationRequest request in DbContextChannel.Reader.ReadAllAsync())
    {
      await request.ExecuteAsync();
    }
  });

  public bool IsDisposed => _disposed;

  private bool _disposed;

  private void Dispose(bool disposing)
  {
    if (!_disposed)
    {
      if (disposing)
      {
        DbContextChannel.Writer.TryComplete();
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