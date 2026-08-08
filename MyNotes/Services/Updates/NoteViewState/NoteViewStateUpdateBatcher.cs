using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Debugging.Attributes;

namespace MyNotes.Services.Updates.NoteViewState;

[ReferenceTracker]
internal sealed partial class NoteViewStateUpdateBatcher : IUpdateBatcher<string, NoteViewStatePatchDto>
{
  private readonly TimeProvider BatchTimeProvider;
  private readonly IUpdateBatchDispatcher<NoteViewStatePatchDto> ViewStatePersistenceDispatcher;

  private readonly TimeSpan _batchTimeSpan = TimeSpan.FromMilliseconds(3000);
  private ITimer? _batchTimer;
  private readonly Dictionary<string, NoteViewStatePatchDto> _pendingEntries = [];
  private bool HasPendingPatch => _pendingEntries.Count > 0;
  private readonly Lock _pendingLock = new();

  public NoteViewStateUpdateBatcher(TimeProvider timeProvider, IUpdateBatchDispatcher<NoteViewStatePatchDto> viewStatePersistenceDispatcher)
  {
    BatchTimeProvider = timeProvider;
    ViewStatePersistenceDispatcher = viewStatePersistenceDispatcher;
    TrackReference();
  }

  public void AddOrMerge(string key, NoteViewStatePatchDto patch)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
    lock (_pendingLock)
    {
      if (!HasPendingPatch)
      {
        _timestamp = BatchTimeProvider.GetTimestamp();
        _batchTimer ??= BatchTimeProvider.CreateTimer(OnTimerElapsed, this, _batchTimeSpan, Timeout.InfiniteTimeSpan);
      }

      _pendingEntries[key] = patch;
    }
  }

  private long _timestamp;

  private async void OnTimerElapsed(object? state)
  {
    Console.WriteLine($"OnTimerElapsed entered: {DateTimeOffset.Now:O}, " + $"elapsed: {BatchTimeProvider.GetElapsedTime(_timestamp)}");
    Flush();
  }

  public void Flush()
  {
    lock (_pendingLock)
    {
      if (HasPendingPatch)
      {
        var patch = NoteViewStatePatchDto.Composite(_pendingEntries.Values);
        ViewStatePersistenceDispatcher.TryEnqueue(patch);
        _pendingEntries.Clear();
        _batchTimer?.Dispose();
      }

      _batchTimer = null;
    }
  }

  private bool _disposeStarted;

  private async ValueTask DisposeAsyncCore()
  {
    if (Interlocked.Exchange(ref _disposeStarted, true))
    {
      return;
    }

    Console.WriteLine("{0}: {1}", "Batcher Disposing", true);
    if (_batchTimer is not null)
    {
      await _batchTimer.DisposeAsync();
      Flush();
    }
  }

  public async ValueTask DisposeAsync()
  {
    await DisposeAsyncCore().ConfigureAwait(false);
    GC.SuppressFinalize(this);
  }
}