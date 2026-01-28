using MyNotes.Models.Navigations;

namespace MyNotes.ViewModels.Navigations;

internal abstract class NavigationViewModelBase : ViewModelBase, INavigationViewModel
{
  public abstract INavigation Navigation { get; }
}
