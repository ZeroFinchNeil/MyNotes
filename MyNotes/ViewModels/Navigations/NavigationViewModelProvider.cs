using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Navigations;

namespace MyNotes.ViewModels.Navigations;

internal sealed class NavigationViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<INavigation, NavigationViewModelBase>
{
  private readonly IServiceProvider ServiceProvider = serviceProvider;

  private readonly Dictionary<INavigation, WeakReference<NavigationViewModelBase>> ResolvedViewModels = new();

  public NavigationViewModelBase Resolve(INavigation navigation)
  {
    if (ResolvedViewModels.TryGetValue(navigation, out var wr)
      && wr.TryGetTarget(out var viewmodel))
    {
      return viewmodel;
    }

    var scope = ServiceProvider.CreateScope();
    NavigationViewModelBase newViewModel = navigation switch
    {
      NavigationCoreNode => ActivatorUtilities.CreateInstance<CoreNavigationViewModel>(scope.ServiceProvider, scope, navigation),
      NavigationSeparator => ActivatorUtilities.CreateInstance<SeparatorNavigationViewModel>(scope.ServiceProvider, scope, navigation),
      NavigationUserRootNode => ActivatorUtilities.CreateInstance<UserRootNavigationViewModel>(scope.ServiceProvider, scope, navigation),
      NavigationUserCompositeNode => ActivatorUtilities.CreateInstance<UserCompositeNavigationViewModel>(scope.ServiceProvider, scope, navigation),
      NavigationUserLeafNode => ActivatorUtilities.CreateInstance<UserLeafNavigationViewModel>(scope.ServiceProvider, scope, navigation),
      _ => throw new ArgumentException("Invalid navigation")
    };

    ResolvedViewModels[navigation] = new WeakReference<NavigationViewModelBase>(newViewModel);

    return newViewModel;
  }

  public bool TryResolve(INavigation navigation, out NavigationViewModelBase? viewmodelbase)
  {
    if (ResolvedViewModels.TryGetValue(navigation, out var wr)
      && wr.TryGetTarget(out var viewmodel))
    {
      viewmodelbase = viewmodel;
      return true;
    }

    viewmodelbase = null;
    return false;
  }

  public bool Release(INavigation navigation)
  {
    if (ResolvedViewModels.TryGetValue(navigation, out var wr)
      && wr.TryGetTarget(out var viewmodel))
    {
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
        viewmodel.Dispose();
      }
    }
    ResolvedViewModels.Clear();
  }
}
