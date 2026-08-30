using System.Collections.Concurrent;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Lifetime;
using MyNotes.Models.Media;

namespace MyNotes.ViewModels.Media.Providers;

internal class ImageCollectionViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<ImageCollectionKey, ImageCollectionViewModel>
{
  private readonly ConcurrentDictionary<ImageCollectionKey, ViewModelCache> ResolveTable = new();
  private readonly Func<ImageCollectionKey, ViewModelCache> _cacheFactory = key => new
  (
    referenceCounterFactory: () => new ReferenceCounter<ImageCollectionViewModel>(ActivatorUtilities.CreateInstance<ImageCollectionViewModel>(serviceProvider, key))
  );

  public IViewModelLease<ImageCollectionViewModel> Resolve(ImageCollectionKey key)
  {
    while (true)
    {
      var cache = ResolveTable.GetOrAdd(key, _cacheFactory.Invoke);

      lock (cache.SyncRoot)
      {
        if (ResolveTable.TryGetValue(key, out ViewModelCache? currentCache) && ReferenceEquals(currentCache, cache))
        {
          if (cache.ReferenceCounter.TryAcquire(out var viewModel))
          {
            return CreateLease(key, viewModel, cache);
          }

          ResolveTable.TryRemove(key, out _);
        }
      }
    }
  }

  private ViewModelLease CreateLease(ImageCollectionKey key, ImageCollectionViewModel viewmodel, ViewModelCache cache) => new ViewModelLease()
  {
    ViewModel = viewmodel,
    ReleaseAction = () =>
    {
      lock (cache.SyncRoot)
      {
        if (cache.ReferenceCounter.ReleaseOrDetach(out _))
        {
          viewmodel.Dispose();
          ResolveTable.TryRemove(key, out _);
        }
      }
    }
  };

  public IViewModelLease<ImageCollectionViewModel>? Acquire(ImageCollectionKey key)
  {
    if (ResolveTable.TryGetValue(key, out var cache))
    {
      lock (cache.SyncRoot)
      {
        if (cache.ReferenceCounter.TryAcquire(out var viewmodel))
        {
          if (!viewmodel.Disposed)
          {
            return CreateLease(key, viewmodel, cache);
          }
          else
          {
            ResolveTable.TryRemove(key, out _);
          }
        }
      }
    }
    return null;
  }

  private sealed class ViewModelLease() : IViewModelLease<ImageCollectionViewModel>
  {
    public required ImageCollectionViewModel ViewModel { get; init; }
    public required Action ReleaseAction { get; init; }

    public bool Disposed { get; private set; }

    private void Dispose(bool disposing)
    {
      if (Disposed)
      {
        return;
      }

      if (disposing)
      {
        ReleaseAction.Invoke();
      }

      Disposed = true;
    }

    public void Dispose()
    {
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }

  private sealed class ViewModelCache(Func<ReferenceCounter<ImageCollectionViewModel>> referenceCounterFactory)
  {
    public Lock SyncRoot { get; } = new();

    private readonly Lazy<ReferenceCounter<ImageCollectionViewModel>> _referenceCounter = new(referenceCounterFactory);

    public ReferenceCounter<ImageCollectionViewModel> ReferenceCounter => _referenceCounter.Value;
  }
}