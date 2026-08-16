using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Lifetime;
using MyNotes.Debugging;
using MyNotes.Domain.Notes;
using MyNotes.Models.Notes;

namespace MyNotes.ViewModels.Notes.Providers;

internal sealed class NoteViewModelProvider(IServiceProvider serviceProvider) : IAsyncViewModelProvider<NoteModel, NoteViewModel>
{
  private readonly ConcurrentDictionary<NoteModel, NoteViewModelCache> ResolveTable = new();

  private readonly Func<NoteModel, NoteViewModelCache> _cacheFactory = noteModel => new
  (
    referenceCounterFactory: () => new ReferenceCounter<NoteViewModel>(ActivatorUtilities.CreateInstance<NoteViewModel>(serviceProvider, noteModel)),
    serviceScopeFactory: () => serviceProvider.CreateAsyncScope()
  );

  public async Task<IAsyncViewModelLease<NoteViewModel>> ResolveAsync(NoteModel noteModel)
  {
    var cache = ResolveTable.GetOrAdd(noteModel, nav => _cacheFactory(nav));

    await cache.Semaphore.WaitAsync();
    try
    {
      if (cache.ReferenceCounter.TryAcquire(out var viewmodel))
      {
        return CreateLease(noteModel, viewmodel, cache);
      }
    }
    finally
    {
      cache.Semaphore.Release();
    }

    NoteViewModelCache newCache = _cacheFactory(noteModel);

    await newCache.Semaphore.WaitAsync();
    try
    {
      ResolveTable.AddOrUpdate(noteModel, newCache, (k, v) => v = newCache);
      return newCache.ReferenceCounter.TryAcquire(out var viewmodel) ? CreateLease(noteModel, viewmodel, newCache) : throw new InvalidOperationException();
    }
    finally
    {
      newCache.Semaphore.Release();
    }

    throw new InvalidOperationException();
  }

  public async Task<IAsyncViewModelLease<NoteViewModel>?> AcquireAsync(NoteModel noteModel)
  {
    if (ResolveTable.TryGetValue(noteModel, out var cache))
    {
      await cache.Semaphore.WaitAsync();
      try
      {
        if (cache.ReferenceCounter.TryAcquire(out var viewmodel))
        {
          if (!viewmodel.Disposed)
          {
            return CreateLease(noteModel, viewmodel, cache);
          }
          else
          {
            ResolveTable.TryRemove(noteModel, out _);
          }
        }
      }
      finally
      {
        cache.Semaphore.Release();
      }
    }
    return null;
  }

  private NoteViewModelLease CreateLease(NoteModel noteModel, NoteViewModel viewmodel, NoteViewModelCache cache) => new NoteViewModelLease()
  {
    ViewModel = viewmodel,
    ReleaseFunc = () => ReleaseAsync(noteModel, cache)
  };

  private async Task<bool> ReleaseAsync(NoteModel noteModel, NoteViewModelCache cache)
  {
    await cache.Semaphore.WaitAsync();
    try
    {
      if (cache.ReferenceCounter.ReleaseOrDetach(out _))
      {
        await cache.ServiceScope.DisposeAsync();
        ResolveTable.TryRemove(noteModel, out _);
        return true;
      }
      return false;
    }
    finally
    {
      cache.Semaphore.Release();
    }
  }

  private class NoteViewModelLease : IAsyncViewModelLease<NoteViewModel>
  {
    public required NoteViewModel ViewModel { get; init; }
    public required Func<Task<bool>>? ReleaseFunc { get; init; }

    private bool _disposeStarted;
    private async ValueTask DisposeAsyncCore()
    {
      if (Interlocked.Exchange(ref _disposeStarted, true))
      {
        return;
      }

      if (ReleaseFunc is null || await ReleaseFunc.Invoke())
      {
        await ViewModel.DisposeAsync();
      }
    }

    public async ValueTask DisposeAsync()
    {
      await DisposeAsyncCore().ConfigureAwait(false);
      GC.SuppressFinalize(this);
    }
  }

  private sealed class NoteViewModelCache(Func<ReferenceCounter<NoteViewModel>> referenceCounterFactory, Func<AsyncServiceScope> serviceScopeFactory)
  {
    public SemaphoreSlim Semaphore { get; } = new(1, 1);

    private readonly Lazy<ReferenceCounter<NoteViewModel>> _referenceCounter = new(referenceCounterFactory);
    private readonly Lazy<AsyncServiceScope> _serviceScope = new(serviceScopeFactory);

    public ReferenceCounter<NoteViewModel> ReferenceCounter => _referenceCounter.Value;

    public AsyncServiceScope ServiceScope => _serviceScope.Value;
  }
}