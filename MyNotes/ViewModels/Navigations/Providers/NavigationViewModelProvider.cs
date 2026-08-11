using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Domain.Navigations;
using MyNotes.Models.Navigations;

namespace MyNotes.ViewModels.Navigations.Providers;

internal sealed class NavigationViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<INavigation, NavigationViewModelBase>
{
  private readonly IServiceProvider ServiceProvider = serviceProvider;

  private readonly Dictionary<INavigation, NavigationViewModelBase> ResolvedViewModels = new();

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
      NavigationUserRootNode => ActivatorUtilities.CreateInstance<UserRootGroupNavigationViewModel>(ServiceProvider, navigation),
      NavigationUserCompositeNode => ActivatorUtilities.CreateInstance<UserGroupNavigationViewModel>(ServiceProvider, navigation),
      NavigationUserLeafNode => ActivatorUtilities.CreateInstance<UserListNavigationViewModel>(ServiceProvider, navigation),
      NavigationSearch => ActivatorUtilities.CreateInstance<SearchNavigationViewModel>(ServiceProvider, navigation),
      _ => throw new ArgumentException("Invalid navigation")
    };

    ResolvedViewModels[navigation] = newViewModel;

    return newViewModel;
  }

  public IReadOnlyList<NavigationViewModelBase> Resolve(IEnumerable<INavigation> navigations) => [.. navigations.Select(Resolve)];

  public IReadOnlyList<T> Resolve<T>(IEnumerable<INavigation> navigations) where T : NavigationViewModelBase => [.. navigations.Select(Resolve).OfType<T>()];

  public bool TryResolve(INavigation navigation, [NotNullWhen(true)] out NavigationViewModelBase? viewmodelBase)
  {
    if (ResolvedViewModels.TryGetValue(navigation, out var viewmodel)
        && !viewmodel.Disposed)
    {
      viewmodelBase = viewmodel;
      return true;
    }

    viewmodelBase = null;
    return false;
  }

  public bool TryResolve(NavigationId navigationId, [NotNullWhen(true)] out NavigationViewModelBase? viewmodelBase)
  {
    if (ResolvedViewModels.Keys.FirstOrDefault(k => k is INavigationNode n && n.Id == navigationId) is INavigation navigation
      && ResolvedViewModels.TryGetValue(navigation, out var viewmodel))
    {
      viewmodelBase = viewmodel;
      return true;
    }

    viewmodelBase = null;
    return false;
  }

  public bool Release(INavigation navigation)
  {
    if (TryResolve(navigation, out var viewmodel))
    {
      if (!viewmodel.Disposed)
      {
        viewmodel.Dispose();
      }

      ResolvedViewModels.Remove(navigation);
    }
    return false;
  }
}
