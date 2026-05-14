using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using MyNotes.Common.Operations;

namespace MyNotes.Infrastructure.Database.Core;

internal sealed partial class AppDbContextTaskDispatcher : IDisposable
{
  private readonly Channel<IOperationRequest> DbContextChannel = Channel.CreateUnbounded<IOperationRequest>(new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false });

  public AppDbContextTaskDispatcher()
  {
    _ = RunWorker();
  }

  public async Task<int> EnqueueSaveChangesAsync(Func<int> saveChanges, CancellationToken cancellationToken = default)
  {
    DbContextSaveChangesOperationRequest request = new(saveChanges);
    await DbContextChannel.Writer.WriteAsync(request, cancellationToken);
    return await request.TaskCompletionSource.Task;
  }

  private Task RunWorker() => Task.Run(async () =>
  {
    await foreach (IOperationRequest request in DbContextChannel.Reader.ReadAllAsync())
    {
      request.Execute();
    }
  });

  public bool Disposed { get; private set; }

  private void Dispose(bool disposing)
  {
    if (!Disposed)
    {
      if (disposing)
      {
        DbContextChannel.Writer.TryComplete();
      }
      Disposed = true;
    }
  }

  public void Dispose()
  {
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }
}