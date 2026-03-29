using MyNotes.Models.Navigations;

namespace MyNotes.ViewModels.Navigations;

internal sealed partial class SeparatorNavigationViewModel : NavigationViewModelBase
{
  public override NavigationSeparator Navigation { get; }

  public SeparatorNavigationViewModel(NavigationSeparator navigation)
  {
    Navigation = navigation;
  }

  protected override void Dispose(bool disposing)
  {
    if (Disposed)
      return;

    if (disposing)
    {
    }

    base.Dispose(disposing);
  }
}
