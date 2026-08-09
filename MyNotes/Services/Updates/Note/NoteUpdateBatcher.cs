using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Notes.Results;
using MyNotes.Application.Results;
using MyNotes.Common.Operations;

namespace MyNotes.Services.Updates.Note;

internal sealed class NoteUpdateBatcher : IUpdateBatcher<string, NotePatchDto, UpdateNoteResult>
{
  private readonly TimeProvider BatchTimeProvider;
  private readonly IUpdateDispatcher<NotePatchDto, UpdateNoteResult> NoteUpdateDispatcher;

  private readonly TimeSpan _batchTimeSpan = TimeSpan.FromMilliseconds(3000);
  private ITimer? _batchTimer;
  private readonly Dictionary<string, NotePatchOperationRequest> _pendingEntries = [];
  private bool HasPendingPatch => _pendingEntries.Count > 0;
  private readonly SemaphoreSlim _pendingSemaphore = new(1, 1);

  public NoteUpdateBatcher(TimeProvider timeProvider, IUpdateDispatcher<NotePatchDto, UpdateNoteResult> noteUpdateDispatcher)
  {
    BatchTimeProvider = timeProvider;
    NoteUpdateDispatcher = noteUpdateDispatcher;
  }

  public async Task<UpdateNoteResult> AddOrMergeAsync(string key, NotePatchDto patch, CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

    await _pendingSemaphore.WaitAsync(cancellationToken);

    if (!HasPendingPatch)
    {
      _timestamp = BatchTimeProvider.GetTimestamp();
      _batchTimer ??= BatchTimeProvider.CreateTimer(OnTimerElapsed, this, _batchTimeSpan, Timeout.InfiniteTimeSpan);
    }

    if (_pendingEntries.TryGetValue(key, out var pendingRequest))
    {
      pendingRequest.Cancel();
    }

    var newRequest = new NotePatchOperationRequest
    (
      operation: () => NoteUpdateDispatcher.DispatchAsync(patch),
      fallbackValue: new UpdateNoteResult() { Status = AppUpdateStatus.Failed }
    );
    _pendingEntries[key] = newRequest;

    _pendingSemaphore.Release();

    try
    {
      return await newRequest.Result;
    }
    catch (OperationCanceledException)
    {
      return new() { Status = AppUpdateStatus.Canceled };
    }
    catch
    {
      return new() { Status = AppUpdateStatus.Failed };
    }
  }

  private long _timestamp;

  private async void OnTimerElapsed(object? state)
  {
    Console.WriteLine($"OnTimerElapsed entered: {DateTimeOffset.Now:O}, " + $"elapsed: {BatchTimeProvider.GetElapsedTime(_timestamp)}");
    await FlushAsync();
  }

  public async Task FlushAsync(CancellationToken cancellationToken = default)
  {
    await _pendingSemaphore.WaitAsync(cancellationToken);

    if (HasPendingPatch)
    {
      foreach (var request in _pendingEntries.Values)
      {
        await request.ExecuteAsync();
      }
      _pendingEntries.Clear();
      _batchTimer?.Dispose();
    }

    _batchTimer = null;
    _pendingSemaphore.Release();
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
      await FlushAsync();
    }
  }

  public async ValueTask DisposeAsync()
  {
    await DisposeAsyncCore().ConfigureAwait(false);
    GC.SuppressFinalize(this);
  }

  private class NotePatchOperation
  {
    public required NotePatchDto Patch { get; set; }

    public required Func<Task<UpdateNoteResult>> Operation { get; init; }
  }
}

internal sealed class NotePatchOperationRequest(Func<Task<UpdateNoteResult>> operation, UpdateNoteResult fallbackValue) : AsyncOperationRequest<UpdateNoteResult>(operation, fallbackValue)
{
  public override async Task ExecuteAsync()
  {
    try
    {
      var result = await Operation.Invoke();
      Console.WriteLine("{0}: {1}", "NotePatchOperationRequest invoked", result);
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

  public void Cancel()
  {
    if (TaskCompletionSource.Task.IsCanceled)
    {
      return;
    }
    TaskCompletionSource.TrySetCanceled();
  }
}