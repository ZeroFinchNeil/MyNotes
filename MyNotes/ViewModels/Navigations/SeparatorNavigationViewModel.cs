using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Navigations;

namespace MyNotes.ViewModels.Navigations;

internal sealed partial class SeparatorNavigationViewModel : NavigationViewModelBase
{
  private readonly IServiceScope ServiceScope;
  public override NavigationSeparator Navigation { get; }

  public SeparatorNavigationViewModel(IServiceScope serviceScope, NavigationSeparator navigation)
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
