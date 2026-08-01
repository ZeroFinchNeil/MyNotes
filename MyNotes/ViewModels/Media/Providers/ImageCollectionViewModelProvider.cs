using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Debugging;
using MyNotes.Domain.Notes;

namespace MyNotes.ViewModels.Media.Providers;

internal class ImageCollectionViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<NoteId, ImageCollectionViewModel>
{
  private readonly ConcurrentDictionary<NoteId, ReferenceCounter<ImageCollectionViewModel>> ResolveTable = new();

  public ImageCollectionViewModel Resolve(NoteId key)
  {
    var rc = ResolveTable.GetOrAdd(key, noteId => new ReferenceCounter<ImageCollectionViewModel>()
    {
      Instance = ActivatorUtilities.CreateInstance<ImageCollectionViewModel>(serviceProvider, key)
    });

    rc.Increment();
    return rc.Instance;
  }

  public bool TryResolve(NoteId key, [NotNullWhen(true)] out ImageCollectionViewModel? imageCollectionViewModel)
  {
    if (ResolveTable.TryGetValue(key, out var rc))
    {
      var viewmodel = rc.Instance;
      if (!rc.HasNoReferences && !viewmodel.Disposed)
      {
        imageCollectionViewModel = viewmodel;
        return true;
      }
      else
      {
        ResolveTable.TryRemove(key, out _);
      }
    }
    imageCollectionViewModel = null;
    return false;
  }

  public bool Release(NoteId key) => ResolveTable.TryGetValue(key, out var rc) && rc.Decrement() && ResolveTable.TryRemove(key, out _);
}