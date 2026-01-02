using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Navigations;

namespace MyNotes.ViewModels.Navigations;

internal sealed partial class CoreNavigationViewModel : NavigationViewModelBase
{
  private readonly IServiceScope ServiceScope;
  public override NavigationCoreNode Navigation { get; }

  public CoreNavigationViewModel(IServiceScope serviceScope, NavigationCoreNode navigation)
  {
    ServiceScope = serviceScope;
    Navigation = navigation;
  }

  protected override void Dispose(bool disposing)
  {
    if (_disposed)
      return;

    if (disposing)
    {
      ServiceScope.Dispose();
    }

    _disposed = true;
  }
}
