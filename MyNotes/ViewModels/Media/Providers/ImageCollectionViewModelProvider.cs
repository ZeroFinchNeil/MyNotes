using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

namespace MyNotes.ViewModels.Media.Providers;

internal class ImageCollectionViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<ImageCollectionKey, ImageCollectionViewModel>
{
  private readonly IServiceProvider ServiceProvider = serviceProvider;

  private readonly Dictionary<ImageCollectionKey, WeakReference<ImageCollectionViewModel>> ResolvedViewModels = new();

  public ImageCollectionViewModel Resolve(ImageCollectionKey key)
  {
    if (TryResolve(key, out var viewmodel))
    {
      return viewmodel;
    }

    ImageCollectionViewModel newViewModel = ActivatorUtilities.CreateInstance<ImageCollectionViewModel>(ServiceProvider, key);
    ResolvedViewModels[key] = new WeakReference<ImageCollectionViewModel>(newViewModel);

    return newViewModel;
  }

  public bool TryResolve(ImageCollectionKey key, [NotNullWhen(true)] out ImageCollectionViewModel? imageCollectionViewModel)
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

  public bool Release(ImageCollectionKey key)
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