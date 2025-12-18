using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Navigations;

namespace MyNotes.ViewModels.Navigations;

internal sealed partial class NavigationViewModelFactory(IServiceProvider serviceProvider) : IViewModelFactory
{
  private readonly IServiceProvider ServiceProvider = serviceProvider;

  public NavigationViewModelBase Resolve(INavigation navigation) => navigation switch
  {
    NavigationCoreNode => ActivatorUtilities.CreateInstance<CoreNavigationViewModel>(ServiceProvider, navigation),
    NavigationSeparator => ActivatorUtilities.CreateInstance<SeparatorNavigationViewModel>(ServiceProvider, navigation),
    NavigationUserCompositeNode => ActivatorUtilities.CreateInstance<UserCompositeNavigationViewModel>(ServiceProvider, navigation),
    NavigationUserLeafNode => ActivatorUtilities.CreateInstance<UserLeafNavigationViewModel>(ServiceProvider, navigation),
    _ => throw new ArgumentException("Invalid navigation")
  };
}
