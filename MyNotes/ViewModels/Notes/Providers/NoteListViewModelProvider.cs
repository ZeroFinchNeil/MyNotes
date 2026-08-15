using System.Collections.Concurrent;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Lifetime;
using MyNotes.Domain.Navigations;
using MyNotes.Models.Navigations;

namespace MyNotes.ViewModels.Notes.Providers;

internal sealed class NoteListViewModelProvider(IServiceProvider serviceProvider) : IAsyncViewModelProvider<INavigationNoteList, NoteListViewModel>
{
  private readonly ConcurrentDictionary<INavigationNoteList, NoteListViewModelCache> ResolveTable = new();

  private readonly Func<INavigationNoteList, NoteListViewModelCache> _cacheFactory = navigation => new
  (
    referenceCounterFactory: () => new ReferenceCounter<NoteListViewModel>(ActivatorUtilities.CreateInstance<NoteListViewModel>(serviceProvider, navigation)),
    serviceScopeFactory: () => serviceProvider.CreateAsyncScope()
  );

  public async Task<IAsyncViewModelLease<NoteListViewModel>> ResolveAsync(INavigationNoteList navigation)
  {
    var cache = ResolveTable.GetOrAdd(navigation, _cacheFactory.Invoke);

    await cache.Semaphore.WaitAsync();
    try
    {
      if (cache.ReferenceCounter.TryAcquire(out var viewmodel))
      {
        return CreateLease(navigation, viewmodel, cache);
      }
    }
    finally
    {
      cache.Semaphore.Release();
    }

    NoteListViewModelCache newCache = _cacheFactory(navigation);

    await newCache.Semaphore.WaitAsync();
    try
    {
      ResolveTable.AddOrUpdate(navigation, newCache, (k, v) => v = newCache);
      return newCache.ReferenceCounter.TryAcquire(out var viewmodel) ? CreateLease(navigation, viewmodel, newCache) : throw new InvalidOperationException();
    }
    finally
    {
      newCache.Semaphore.Release();
    }

    throw new InvalidOperationException();
  }

  public async Task<IAsyncViewModelLease<NoteListViewModel>?> AcquireAsync(INavigationNoteList navigation)
  {
    if (ResolveTable.TryGetValue(navigation, out var cache))
    {
      await cache.Semaphore.WaitAsync();
      try
      {
        if (cache.ReferenceCounter.TryAcquire(out var viewmodel))
        {
          if (!viewmodel.Disposed)
          {
            return CreateLease(navigation, viewmodel, cache);
          }
          else
          {
            ResolveTable.TryRemove(navigation, out _);
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

  public async Task<IAsyncViewModelLease<NoteListViewModel>?> AcquireAsync(NavigationId navigationId)
  {
    foreach (var nav in ResolveTable.Keys.ToArray())
    {
      if (nav is INavigationNode node && node.Id == navigationId)
      {
        return await AcquireAsync(nav);
      }
    }

    return null;
  }

  private NoteListViewModelLease CreateLease(INavigationNoteList navigation, NoteListViewModel viewmodel, NoteListViewModelCache cache) => new NoteListViewModelLease()
  {
    ViewModel = viewmodel,
    ReleaseFunc = () => ReleaseAsync(navigation, cache)
  };

  private async Task<bool> ReleaseAsync(INavigationNoteList navigation, NoteListViewModelCache cache)
  {
    await cache.Semaphore.WaitAsync();
    try
    {
      if (cache.ReferenceCounter.ReleaseOrDetach(out _))
      {
        await cache.ServiceScope.DisposeAsync();
        ResolveTable.TryRemove(navigation, out _);
      }
      return true;
    }
    finally
    {
      cache.Semaphore.Release();
    }
  }

  private class NoteListViewModelLease : IAsyncViewModelLease<NoteListViewModel>
  {
    public required NoteListViewModel ViewModel { get; init; }
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

  private sealed class NoteListViewModelCache(Func<ReferenceCounter<NoteListViewModel>> referenceCounterFactory, Func<AsyncServiceScope> serviceScopeFactory)
  {
    public SemaphoreSlim Semaphore { get; } = new(1, 1);

    private readonly Lazy<ReferenceCounter<NoteListViewModel>> _referenceCounter = new(referenceCounterFactory);
    private readonly Lazy<AsyncServiceScope> _serviceScope = new(serviceScopeFactory);

    public ReferenceCounter<NoteListViewModel> ReferenceCounter => _referenceCounter.Value;

    public AsyncServiceScope ServiceScope => _serviceScope.Value;
  }
}
