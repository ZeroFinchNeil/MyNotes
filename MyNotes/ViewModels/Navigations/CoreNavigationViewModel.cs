using MyNotes.Models.Navigations;

namespace MyNotes.ViewModels.Navigations;

internal sealed partial class CoreNavigationViewModel : NavigationViewModelBase
{
  public override NavigationCoreNode Navigation { get; }

  #region Object Lifetime Management
  public CoreNavigationViewModel(NavigationCoreNode navigation)
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
  #endregion
}
