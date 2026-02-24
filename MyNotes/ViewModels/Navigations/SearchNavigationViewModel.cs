using MyNotes.AppConstants;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.Notes;
using MyNotes.Services.Search;
using MyNotes.Services.Settings;
using MyNotes.ViewModels.Notes;

namespace MyNotes.ViewModels.Navigations;

internal sealed class SearchNavigationViewModel : NavigationViewModelBase
{
  public override NavigationSearch Navigation { get; }

  public SearchNavigationViewModel(NavigationSearch navigation)
  {
    Navigation = navigation;
  }
}
