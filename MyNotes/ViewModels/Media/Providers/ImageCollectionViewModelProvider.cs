using System.Collections.Concurrent;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Lifetime;
using MyNotes.Domain.Notes;

namespace MyNotes.ViewModels.Media.Providers;

internal class ImageCollectionViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<NoteId, ImageCollectionViewModel>
{
  private readonly ConcurrentDictionary<NoteId, ImageCollectionViewModelCache> ResolveTable = new();
  private readonly Func<NoteId, ImageCollectionViewModelCache> _cacheFactory = noteId => new
  (
    referenceCounterFactory: () => new ReferenceCounter<ImageCollectionViewModel>(ActivatorUtilities.CreateInstance<ImageCollectionViewModel>(serviceProvider, noteId))
  );

  public IViewModelLease<ImageCollectionViewModel> Resolve(NoteId noteId)
  {
    var cache = ResolveTable.GetOrAdd(noteId, _cacheFactory.Invoke);

    lock (cache.SyncRoot)
    {
      if (cache.ReferenceCounter.TryAcquire(out var viewmodel))
      {
        return CreateLease(noteId, viewmodel, cache);
      }
    }

    ImageCollectionViewModelCache newCache = _cacheFactory(noteId);

    lock (newCache.SyncRoot)
    {
      ResolveTable.AddOrUpdate(noteId, newCache, (k, v) => v = newCache);
      return newCache.ReferenceCounter.TryAcquire(out var viewmodel) ? CreateLease(noteId, viewmodel, newCache) : throw new InvalidOperationException();
    }
  }

  private ImageCollectionViewModelLease CreateLease(NoteId noteId, ImageCollectionViewModel viewmodel, ImageCollectionViewModelCache cache) => new ImageCollectionViewModelLease()
  {
    ViewModel = viewmodel,
    ReleaseFunc = () => Release(noteId, cache)
  };

  public IViewModelLease<ImageCollectionViewModel>? Acquire(NoteId noteId)
  {
    if (ResolveTable.TryGetValue(noteId, out var cache))
    {
      lock (cache.SyncRoot)
      {
        if (cache.ReferenceCounter.TryAcquire(out var viewmodel))
        {
          if (!viewmodel.Disposed)
          {
            return CreateLease(noteId, viewmodel, cache);
          }
          else
          {
            ResolveTable.TryRemove(noteId, out _);
          }
        }
      }
    }
    return null;
  }

  private bool Release(NoteId noteId, ImageCollectionViewModelCache cache)
  {
    lock (cache.SyncRoot)
    {
      if (cache.ReferenceCounter.ReleaseOrDetach(out _))
      {
        ResolveTable.TryRemove(noteId, out _);
        return true;
      }
      return false;
    }
  }

  private sealed class ImageCollectionViewModelLease() : IViewModelLease<ImageCollectionViewModel>
  {
    public required ImageCollectionViewModel ViewModel { get; init; }
    public Func<bool>? ReleaseFunc { get; init; }

    public bool Disposed { get; private set; }

    private void Dispose(bool disposing)
    {
      if (Disposed)
      {
        return;
      }

      if (disposing)
      {
        if (ReleaseFunc is null || ReleaseFunc.Invoke())
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

  private sealed class ImageCollectionViewModelCache(Func<ReferenceCounter<ImageCollectionViewModel>> referenceCounterFactory)
  {
    public Lock SyncRoot { get; } = new();

    private readonly Lazy<ReferenceCounter<ImageCollectionViewModel>> _referenceCounter = new(referenceCounterFactory);

    public ReferenceCounter<ImageCollectionViewModel> ReferenceCounter => _referenceCounter.Value;
  }
}