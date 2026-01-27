using MyNotes.Debugging;
using MyNotes.Models.Navigations;

namespace MyNotes.ViewModels.Navigations;

internal abstract class NavigationViewModelBase : ViewModelBase
{
  public abstract INavigation Navigation { get; }
}
