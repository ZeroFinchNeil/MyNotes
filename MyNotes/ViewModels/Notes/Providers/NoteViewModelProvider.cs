using System.Collections.Concurrent;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Lifetime;
using MyNotes.Models.Notes;

namespace MyNotes.ViewModels.Notes.Providers;

internal sealed class NoteViewModelProvider(IServiceProvider serviceProvider) : IAsyncViewModelProvider<NoteModel, NoteViewModel>
{
  private readonly ConcurrentDictionary<NoteModel, ViewModelCache> ResolveTable = new();

  private readonly Func<NoteModel, ViewModelCache> _cacheFactory = noteModel => new ViewModelCache(() =>
  {
    AsyncServiceScope serviceScope = serviceProvider.CreateAsyncScope();
    return new ReferenceCountedViewModelScope
    (
      referenceCounter: new ReferenceCounter<NoteViewModel>(ActivatorUtilities.CreateInstance<NoteViewModel>(serviceScope.ServiceProvider, noteModel)),
      serviceScope: serviceScope
    );
  });

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

    ViewModelCache newCache = _cacheFactory(noteModel);

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

  private ViewModelLease CreateLease(NoteModel noteModel, NoteViewModel viewmodel, ViewModelCache cache) => new ViewModelLease()
  {
    ViewModel = viewmodel,
    ReleaseFunc = () => ReleaseAsync(noteModel, cache)
  };

  private async Task<bool> ReleaseAsync(NoteModel noteModel, ViewModelCache cache)
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

  private class ViewModelLease : IAsyncViewModelLease<NoteViewModel>
  {
    public required NoteViewModel ViewModel { get; init; }
    public required Func<Task<bool>> ReleaseFunc { get; init; }

    private bool _disposeStarted;
    private async ValueTask DisposeAsyncCore()
    {
      if (Interlocked.Exchange(ref _disposeStarted, true))
      {
        return;
      }

      if (await ReleaseFunc.Invoke())
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

  private sealed class ViewModelCache(Func<ReferenceCountedViewModelScope> countedScopeFactory)
  {
    public SemaphoreSlim Semaphore { get; } = new(1, 1);

    private readonly Lazy<ReferenceCountedViewModelScope> _countedScope = new(countedScopeFactory);

    public ReferenceCounter<NoteViewModel> ReferenceCounter => _countedScope.Value.ReferenceCounter;

    public AsyncServiceScope ServiceScope => _countedScope.Value.ServiceScope;
  }

  public sealed class ReferenceCountedViewModelScope(ReferenceCounter<NoteViewModel> referenceCounter, AsyncServiceScope serviceScope)
  {
    public ReferenceCounter<NoteViewModel> ReferenceCounter { get; } = referenceCounter;

    public AsyncServiceScope ServiceScope { get; } = serviceScope;
  }
}