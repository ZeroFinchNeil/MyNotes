using MyNotes.Models.Navigations;

namespace MyNotes.ViewModels.Navigations;

internal abstract class NavigationViewModelBase : ViewModelBase
{
  public virtual INavigation? Navigation { get;}
}
