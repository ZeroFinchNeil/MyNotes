using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Domain.Notes;

namespace MyNotes.ViewModels.Media.Providers;

internal class ImageCollectionViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<NoteId, ImageCollectionViewModel>
{
  private readonly IServiceProvider ServiceProvider = serviceProvider;

  private readonly Dictionary<NoteId, WeakReference<ImageCollectionViewModel>> ResolvedViewModels = new();

  public ImageCollectionViewModel Resolve(NoteId key)
  {
    if (TryResolve(key, out var viewmodel))
    {
      return viewmodel;
    }

    ImageCollectionViewModel newViewModel = ActivatorUtilities.CreateInstance<ImageCollectionViewModel>(ServiceProvider, key);
    ResolvedViewModels[key] = new WeakReference<ImageCollectionViewModel>(newViewModel);

    return newViewModel;
  }

  public bool TryResolve(NoteId key, [NotNullWhen(true)] out ImageCollectionViewModel? imageCollectionViewModel)
  {
    if (ResolvedViewModels.TryGetValue(key, out var wr)
        && wr.TryGetTarget(out var viewmodel)
        && !viewmodel.Disposed)
    {
      imageCollectionViewModel = viewmodel;
      return true;
    }

    imageCollectionViewModel = null;
    return false;
  }

  public bool Release(NoteId key)
  {
    if (TryResolve(key, out var viewmodel))
    {
      if (!viewmodel.Disposed)
      {
        viewmodel.Dispose();
      }

      ResolvedViewModels.Remove(key);
    }
    return false;
  }
}