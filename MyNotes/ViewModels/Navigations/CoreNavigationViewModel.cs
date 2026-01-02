using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Navigations;

namespace MyNotes.ViewModels.Navigations;

internal sealed partial class CoreNavigationViewModel : NavigationViewModelBase
{
  public override NavigationCoreNode Navigation { get; }

  public CoreNavigationViewModel(NavigationCoreNode navigation)
  {
    Navigation = navigation;
  }

  protected override void Dispose(bool disposing)
  {
    if (_disposed)
      return;

    if (disposing)
    {
    }

    _disposed = true;
  }
}
