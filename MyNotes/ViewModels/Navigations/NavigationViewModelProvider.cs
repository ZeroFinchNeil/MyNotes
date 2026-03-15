using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Navigations;

namespace MyNotes.ViewModels.Navigations;

internal sealed class NavigationViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<INavigation, NavigationViewModelBase>
{
  private readonly IServiceProvider ServiceProvider = serviceProvider;

  private readonly Dictionary<INavigation, WeakReference<NavigationViewModelBase>> ResolvedViewModels = new();

  public NavigationViewModelBase Resolve(INavigation navigation)
  {
    if (TryResolve(navigation, out var viewmodel))
    {
      return viewmodel;
    }

    NavigationViewModelBase newViewModel = navigation switch
    {
      NavigationCoreNode => ActivatorUtilities.CreateInstance<CoreNavigationViewModel>(ServiceProvider, navigation),
      NavigationSeparator => ActivatorUtilities.CreateInstance<SeparatorNavigationViewModel>(ServiceProvider, navigation),
      NavigationUserRootNode => ActivatorUtilities.CreateInstance<UserRootNavigationViewModel>(ServiceProvider, navigation),
      NavigationUserCompositeNode => ActivatorUtilities.CreateInstance<UserCompositeNavigationViewModel>(ServiceProvider, navigation),
      NavigationUserLeafNode => ActivatorUtilities.CreateInstance<UserLeafNavigationViewModel>(ServiceProvider, navigation),
      NavigationSearch => ActivatorUtilities.CreateInstance<SearchNavigationViewModel>(ServiceProvider, navigation),
      _ => throw new ArgumentException("Invalid navigation")
    };

    ResolvedViewModels[navigation] = new WeakReference<NavigationViewModelBase>(newViewModel);

    return newViewModel;
  }

  public IReadOnlyList<NavigationViewModelBase> Resolve(IEnumerable<INavigation> navigations) => [..navigations.Select(Resolve)];

  public IReadOnlyList<T> Resolve<T>(IEnumerable<INavigation> navigations) where T : NavigationViewModelBase => [.. navigations.Select(Resolve).OfType<T>()];

  public bool TryResolve(INavigation navigation, [NotNullWhen(true)] out NavigationViewModelBase? viewmodelbase)
  {
    if (ResolvedViewModels.TryGetValue(navigation, out var wr)
        && wr.TryGetTarget(out var viewmodel)
        && !viewmodel.Disposed)
    {
      viewmodelbase = viewmodel;
      return true;
    }

    viewmodelbase = null;
    return false;
  }

  public bool TryResolve(NavigationId navigationId, [NotNullWhen(true)] out NavigationViewModelBase? viewmodelbase)
  {
    if (ResolvedViewModels.Keys.FirstOrDefault(k => k is INavigationNode n && n.Id == navigationId) is INavigation navigation
      && ResolvedViewModels[navigation].TryGetTarget(out var viewmodel))
    {
      viewmodelbase = viewmodel;
      return true;
    }

    viewmodelbase = null;
    return false;
  }

  public bool Release(INavigation navigation)
  {
    if (TryResolve(navigation, out var viewmodel))
    {
      if (!viewmodel.Disposed)
        viewmodel.Dispose();
      ResolvedViewModels.Remove(navigation);
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
