using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Media;
namespace MyNotes.ViewModels.Media.Providers;

internal class ImageViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<ImageDescriptor, ImageViewModel>
{
  private readonly IServiceProvider ServiceProvider = serviceProvider;

  private readonly ConditionalWeakTable<ImageDescriptor, WeakReference<ImageViewModel>> ResolveTable = new();

  public ImageViewModel Resolve(ImageDescriptor descriptor)
  {
    if (TryResolve(descriptor, out var viewmodel))
    {
      return viewmodel;
    }

    ImageViewModel newViewModel = ActivatorUtilities.CreateInstance<ImageViewModel>(ServiceProvider, descriptor);

    ResolveTable.AddOrUpdate(descriptor, new WeakReference<ImageViewModel>(newViewModel));
    return newViewModel;
  }

  public bool TryResolve(ImageDescriptor descriptor, [NotNullWhen(true)] out ImageViewModel? noteImageViewModel)
  {
    if (ResolveTable.TryGetValue(descriptor, out var wr)
        && wr.TryGetTarget(out var viewmodel)
        && !viewmodel.Disposed)
    {
      noteImageViewModel = viewmodel;
      return true;
    }

    noteImageViewModel = null;
    return false;
  }

  public bool Release(ImageDescriptor descriptor)
  {
    if (TryResolve(descriptor, out var viewmodel))
    {
      if (!viewmodel.Disposed)
        viewmodel.Dispose();
      ResolveTable.Remove(descriptor);
    }
    return false;
  }

  public void ReleaseAll()
  {
    foreach (var item in ResolveTable)
    {
      if (item.Value.TryGetTarget(out var viewmodel))
      {
        if (!viewmodel.Disposed)
          viewmodel.Dispose();
      }
    }
    ResolveTable.Clear();
  }
}