using MyNotes.Models.Navigations;

namespace MyNotes.ViewModels.Navigations;

internal sealed class SearchNavigationViewModel : NavigationViewModelBase
{
  public override NavigationSearch Navigation { get; }

  public SearchNavigationViewModel(NavigationSearch navigation)
  {
    Navigation = navigation;
  }
}
