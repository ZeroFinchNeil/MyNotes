using System.Threading.Channels;

using MyNotes.Debugging.Attributes;

namespace MyNotes.Services.Updates;

[ReferenceTracker]
internal abstract partial class UpdateBatchDispatcher<TPatch> : IUpdateBatchDispatcher<TPatch> where TPatch : notnull
{
  private readonly Channel<TPatch> PersistenceChannel = Channel.CreateUnbounded<TPatch>(new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false });

  public UpdateBatchDispatcher()
  {
    TrackReference();
    _workerTask = RunConsumerWorker();
  }

  public bool TryEnqueue(TPatch patch)
  {
    //return PersistenceChannel.Writer.TryWrite(patch);
    var success = PersistenceChannel.Writer.TryWrite(patch);
    Console.WriteLine("{0}: {1}", $"Enqueue({success})", patch);
    return success;
  }

  private readonly Task _workerTask;
  private Task RunConsumerWorker() => Task.Run(async () =>
  {
    await foreach (TPatch patch in PersistenceChannel.Reader.ReadAllAsync())
    {
      Console.WriteLine("{0}: {1}", $"Application Write", patch);
      await WriteAsync(patch);
    }
  });

  protected abstract Task WriteAsync(TPatch patch, CancellationToken cancellationToken = default);

  private bool _disposeStarted;

  public async ValueTask DisposeAsync()
  {
    await DisposeAsyncCore().ConfigureAwait(false);
    GC.SuppressFinalize(this);
  }

  protected virtual async ValueTask DisposeAsyncCore()
  {
    if (Interlocked.Exchange(ref _disposeStarted, true))
    {
      return;
    }

    var completed = PersistenceChannel.Writer.TryComplete();
    Console.WriteLine("{0}: {1}", "Dispatcher Disposing & completed", completed);
    await _workerTask;
  }
}