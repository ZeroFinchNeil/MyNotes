using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Lifetime;
using MyNotes.Domain.Media;
using MyNotes.Models.Media;
namespace MyNotes.ViewModels.Media.Providers;

internal class ImageViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<ImageDescriptor, ImageViewModel>
{
  private readonly ConcurrentDictionary<ImageDescriptor, ViewModelCache> ResolveTable = new();
  private readonly Func<ImageDescriptor, ViewModelCache> _cacheFactory = imageDescriptor => new
  (
    referenceCounterFactory: () => new ReferenceCounter<ImageViewModel>(ActivatorUtilities.CreateInstance<ImageViewModel>(serviceProvider, imageDescriptor))
  );

  public IViewModelLease<ImageViewModel> Resolve(ImageDescriptor imageDescriptor)
  {
    var cache = ResolveTable.GetOrAdd(imageDescriptor, _cacheFactory.Invoke);

    lock (cache.SyncRoot)
    {
      if (cache.ReferenceCounter.TryAcquire(out var viewmodel))
      {
        return CreateLease(imageDescriptor, viewmodel, cache);
      }
    }

    ViewModelCache newCache = _cacheFactory(imageDescriptor);

    lock (newCache.SyncRoot)
    {
      ResolveTable.AddOrUpdate(imageDescriptor, newCache, (k, v) => v = newCache);
      return newCache.ReferenceCounter.TryAcquire(out var viewmodel) ? CreateLease(imageDescriptor, viewmodel, newCache) : throw new InvalidOperationException();
    }
  }

  private ViewModelLease CreateLease(ImageDescriptor imageDescriptor, ImageViewModel viewmodel, ViewModelCache cache) => new ViewModelLease()
  {
    ViewModel = viewmodel,
    ReleaseFunc = () => Release(imageDescriptor, cache)
  };

  public IViewModelLease<ImageViewModel>? Acquire(ImageDescriptor imageDescriptor)
  {
    if (ResolveTable.TryGetValue(imageDescriptor, out var cache))
    {
      lock (cache.SyncRoot)
      {
        if (cache.ReferenceCounter.TryAcquire(out var viewmodel))
        {
          if (!viewmodel.Disposed)
          {
            return CreateLease(imageDescriptor, viewmodel, cache);
          }
          else
          {
            ResolveTable.TryRemove(imageDescriptor, out _);
          }
        }
      }
    }
    return null;
  }

  private bool Release(ImageDescriptor imageDescriptor, ViewModelCache cache)
  {
    lock (cache.SyncRoot)
    {
      if (cache.ReferenceCounter.ReleaseOrDetach(out _))
      {
        ResolveTable.TryRemove(imageDescriptor, out _);
        return true;
      }
      return false;
    }
  }

  private sealed class ViewModelLease() : IViewModelLease<ImageViewModel>
  {
    public required ImageViewModel ViewModel { get; init; }
    public required Func<bool> ReleaseFunc { get; init; }

    public bool Disposed { get; private set; }

    private void Dispose(bool disposing)
    {
      if (Disposed)
      {
        return;
      }

      if (disposing)
      {
        if (ReleaseFunc.Invoke())
        {
          ViewModel.Dispose();
        }
      }

      Disposed = true;
    }

    public void Dispose()
    {
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }

  private sealed class ViewModelCache(Func<ReferenceCounter<ImageViewModel>> referenceCounterFactory)
  {
    public Lock SyncRoot { get; } = new();

    private readonly Lazy<ReferenceCounter<ImageViewModel>> _referenceCounter = new(referenceCounterFactory);

    public ReferenceCounter<ImageViewModel> ReferenceCounter => _referenceCounter.Value;
  }
}