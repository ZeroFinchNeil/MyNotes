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

    NavigationViewModelBase newViewModel = navigation switch
    {
      NavigationCoreNode => ActivatorUtilities.CreateInstance<CoreNavigationViewModel>(ServiceProvider, navigation),
      NavigationSeparator => ActivatorUtilities.CreateInstance<SeparatorNavigationViewModel>(ServiceProvider, navigation),
      NavigationUserCompositeNode => ActivatorUtilities.CreateInstance<UserCompositeNavigationViewModel>(ServiceProvider, navigation),
      NavigationUserLeafNode => ActivatorUtilities.CreateInstance<UserLeafNavigationViewModel>(ServiceProvider, navigation),
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
}
