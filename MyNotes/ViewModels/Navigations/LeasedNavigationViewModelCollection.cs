using MyNotes.ViewModels.Navigations.Items;

namespace MyNotes.ViewModels.Navigations;

internal sealed class LeasedNavigationViewModelCollection : LeasedViewModelCollection<NavigationViewModelBase, ObservableCollection<NavigationViewModelBase>>
{
  public LeasedNavigationViewModelCollection() : base((viewmodels) => new ObservableCollection<NavigationViewModelBase>(viewmodels))
  {
  }

  public LeasedNavigationViewModelCollection(IEnumerable<IViewModelLease<NavigationViewModelBase>> leases) : base(leases, (viewmodels) => new ObservableCollection<NavigationViewModelBase>(viewmodels))
  {
  }
}