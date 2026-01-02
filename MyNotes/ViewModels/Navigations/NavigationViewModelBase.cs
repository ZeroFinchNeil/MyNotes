using MyNotes.Debugging;
using MyNotes.Models.Navigations;

namespace MyNotes.ViewModels.Navigations;

internal abstract class NavigationViewModelBase : ViewModelBase
{
  public virtual INavigation? Navigation { get; }

#if DEBUG
  public NavigationViewModelBase()
  {
    ReferenceTracker.NavigationViewModelReference.Add(this, $"{GetType().Name.Replace("NavigationViewModel", string.Empty),15}   {GetHashCode()}");
  }
#endif
}
