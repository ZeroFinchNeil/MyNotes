using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Lifetime;
using MyNotes.Domain.Notes;

namespace MyNotes.ViewModels.Media.Providers;

internal class ImageCollectionViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<NoteId, ImageCollectionViewModel>
{
  private readonly ConcurrentDictionary<NoteId, ReferenceCounter<ImageCollectionViewModel>> ResolveTable = new();
  private readonly Func<NoteId, ImageCollectionViewModel> _factory = noteId => ActivatorUtilities.CreateInstance<ImageCollectionViewModel>(serviceProvider, noteId);

  public ImageCollectionViewModel Resolve(NoteId noteId)
  {
    var counter = ResolveTable.GetOrAdd(noteId, noteId => new ReferenceCounter<ImageCollectionViewModel>(_factory(noteId)));

    if (counter.TryAcquire(out var viewmodel))
    {
      return viewmodel;
    }

    var newCounter = new ReferenceCounter<ImageCollectionViewModel>(_factory(noteId));
    ResolveTable.AddOrUpdate(noteId, newCounter, (k, v) => v = newCounter);

    return newCounter.TryAcquire(out var newViewModel) ? newViewModel : throw new InvalidOperationException();
  }

  public bool TryResolve(NoteId noteId, [NotNullWhen(true)] out ImageCollectionViewModel? noteViewModel)
  {
    if (ResolveTable.TryGetValue(noteId, out var counter)
      && counter.TryAcquire(out var viewmodel, false))
    {
      if (!viewmodel.Disposed)
      {
        noteViewModel = viewmodel;
        return true;
      }
      else
      {
        ResolveTable.TryRemove(noteId, out _);
      }
    }
    noteViewModel = null;
    return false;
  }

  public bool Release(NoteId noteId)
  {
    if (ResolveTable.TryGetValue(noteId, out var counter))
    {
      if (counter.ReleaseOrDetach(out var viewmodel))
      {
        viewmodel.Dispose();
      }

      return true;
    }

    return false;
  }
}