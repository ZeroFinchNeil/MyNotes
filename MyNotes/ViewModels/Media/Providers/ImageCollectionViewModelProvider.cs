using System.Collections.Concurrent;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Lifetime;
using MyNotes.Domain.Notes;

namespace MyNotes.ViewModels.Media.Providers;

internal class ImageCollectionViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<NoteId, ImageCollectionViewModel>
{
  private readonly ConcurrentDictionary<NoteId, ViewModelCache> ResolveTable = new();
  private readonly Func<NoteId, ViewModelCache> _cacheFactory = noteId => new
  (
    referenceCounterFactory: () => new ReferenceCounter<ImageCollectionViewModel>(ActivatorUtilities.CreateInstance<ImageCollectionViewModel>(serviceProvider, noteId))
  );

  public IViewModelLease<ImageCollectionViewModel> Resolve(NoteId noteId)
  {
    while (true)
    {
      var cache = ResolveTable.GetOrAdd(noteId, _cacheFactory.Invoke);

      lock (cache.SyncRoot)
      {
        if (ResolveTable.TryGetValue(noteId, out ViewModelCache? currentCache) && ReferenceEquals(currentCache, cache))
        {
          if (cache.ReferenceCounter.TryAcquire(out var viewModel))
          {
            return CreateLease(noteId, viewModel, cache);
          }

          ResolveTable.TryRemove(noteId, out _);
        }
      }
    }
  }

  private ViewModelLease CreateLease(NoteId noteId, ImageCollectionViewModel viewmodel, ViewModelCache cache) => new ViewModelLease()
  {
    ViewModel = viewmodel,
    ReleaseAction = () =>
    {
      lock (cache.SyncRoot)
      {
        if (cache.ReferenceCounter.ReleaseOrDetach(out _))
        {
          viewmodel.Dispose();
          ResolveTable.TryRemove(noteId, out _);
        }
      }
    }
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