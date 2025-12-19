using System.Runtime.CompilerServices;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Navigations;

namespace MyNotes.ViewModels.Navigations;

internal sealed partial class NavigationViewModelFactory(IServiceProvider serviceProvider) : IViewModelFactory
{
  private readonly IServiceProvider ServiceProvider = serviceProvider;

  public Dictionary<INavigation, WeakReference<NavigationViewModelBase>> ResolvedViewModels { get; } = new();

  public NavigationViewModelBase Resolve(INavigation navigation)
  {
    NavigationViewModelBase viewModel = navigation switch
    {
      NavigationCoreNode => ActivatorUtilities.CreateInstance<CoreNavigationViewModel>(ServiceProvider, navigation),
      NavigationSeparator => ActivatorUtilities.CreateInstance<SeparatorNavigationViewModel>(ServiceProvider, navigation),
      NavigationUserCompositeNode => ActivatorUtilities.CreateInstance<UserCompositeNavigationViewModel>(ServiceProvider, navigation),
      NavigationUserLeafNode => ActivatorUtilities.CreateInstance<UserLeafNavigationViewModel>(ServiceProvider, navigation),
      _ => throw new ArgumentException("Invalid navigation")
    };

    ResolvedViewModels[navigation] = new WeakReference<NavigationViewModelBase>(viewModel);
    
    return viewModel;
  }
}
