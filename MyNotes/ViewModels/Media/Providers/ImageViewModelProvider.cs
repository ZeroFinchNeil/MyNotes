using System.Collections.Concurrent;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Lifetime;
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
    while (true)
    {
      var cache = ResolveTable.GetOrAdd(imageDescriptor, _cacheFactory.Invoke);

      lock (cache.SyncRoot)
      {
        if (ResolveTable.TryGetValue(imageDescriptor, out ViewModelCache? currentCache) && ReferenceEquals(currentCache, cache))
        {
          if (cache.ReferenceCounter.TryAcquire(out var viewModel))
          {
            return CreateLease(imageDescriptor, viewModel, cache);
          }

          ResolveTable.TryRemove(imageDescriptor, out _);
        }
      }
    }
  }

  private ViewModelLease CreateLease(ImageDescriptor imageDescriptor, ImageViewModel viewmodel, ViewModelCache cache) => new ViewModelLease()
  {
    ViewModel = viewmodel,
    ReleaseAction = () =>
    {
      lock (cache.SyncRoot)
      {
        if (cache.ReferenceCounter.ReleaseOrDetach(out _))
        {
          viewmodel.Dispose();
          ResolveTable.TryRemove(imageDescriptor, out _);
        }
      }
    }
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

  public IEnumerable<IViewModelLease<ImageViewModel>> AcquireAll()
  {
    foreach (var key in ResolveTable.Keys)
    {
      if (Acquire(key) is IViewModelLease<ImageViewModel> lease)
      {
        yield return lease;
      }
    }
  }

  private sealed class ViewModelLease() : IViewModelLease<ImageViewModel>
  {
    public required ImageViewModel ViewModel { get; init; }
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

  private sealed class ViewModelCache(Func<ReferenceCounter<ImageViewModel>> referenceCounterFactory)
  {
    public Lock SyncRoot { get; } = new();

    private readonly Lazy<ReferenceCounter<ImageViewModel>> _referenceCounter = new(referenceCounterFactory);

    public ReferenceCounter<ImageViewModel> ReferenceCounter => _referenceCounter.Value;
  }
}