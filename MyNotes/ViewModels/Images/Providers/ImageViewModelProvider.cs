using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;
namespace MyNotes.ViewModels.Images.Providers;

internal class ImageViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<string, ImageViewModel>
{
  private readonly IServiceProvider ServiceProvider = serviceProvider;

  private readonly Dictionary<string, WeakReference<ImageViewModel>> ResolvedViewModels = new();

  public ImageViewModel? Resolve(string fileName)
  {
    if (TryResolve(fileName, out var viewmodel))
    {
      return viewmodel;
    }

    ImageViewModel newViewModel = ActivatorUtilities.CreateInstance<ImageViewModel>(ServiceProvider, fileName);

    if (newViewModel.LoadSucceeded)
    {
      ResolvedViewModels[fileName] = new WeakReference<ImageViewModel>(newViewModel);
      return newViewModel;
    }

    return null;
  }

  public bool TryResolve(string fileName, [NotNullWhen(true)] out ImageViewModel? noteImageViewModel)
  {
    if (ResolvedViewModels.TryGetValue(fileName, out var wr)
        && wr.TryGetTarget(out var viewmodel)
        && !viewmodel.Disposed)
    {
      noteImageViewModel = viewmodel;
      return true;
    }

    noteImageViewModel = null;
    return false;
  }

  public bool Release(string fileName)
  {
    if (TryResolve(fileName, out var viewmodel))
    {
      if (!viewmodel.Disposed)
        viewmodel.Dispose();
      ResolvedViewModels.Remove(fileName);
    }
    return false;
  }

  public void ReleaseAll()
  {
    foreach (var wr in ResolvedViewModels.Values)
    {
      if (wr.TryGetTarget(out var viewmodel))
      {
        if (!viewmodel.Disposed)
          viewmodel.Dispose();
      }
    }
    ResolvedViewModels.Clear();
  }
}