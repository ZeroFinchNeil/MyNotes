using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Navigations;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Navigations.Providers;
using MyNotes.ViewModels.Notes;
using MyNotes.ViewModels.Notes.Providers;

namespace MyNotes.Views.Navigations;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class SearchResultsPage : Page
{
  private NavigationSearch? Navigation;
  private IViewModelLease<NavigationViewModelBase>? ViewModelLease;
  private IAsyncViewModelLease<NotePreviewListViewModel>? NotePreviewListViewModelLease;

  private SearchNavigationViewModel? ViewModel => ViewModelLease?.ViewModel as SearchNavigationViewModel;
  private NotePreviewListViewModel? NotePreviewListViewModel => NotePreviewListViewModelLease?.ViewModel;

  #region Object Lifetime Management
  public SearchResultsPage()
  {
    TrackReference();
    InitializeComponent();

    this.Loaded += SearchResultsPage_Loaded;
    this.Unloaded += SearchResultsPage_Unloaded;
  }

  protected override async void OnNavigatedTo(NavigationEventArgs e)
  {
    if (e.Parameter is NavigationSearch navigation)
    {
      Navigation = navigation;
      var navigationViewModelProvider = App.Services.GetRequiredService<NavigationViewModelProvider>();
      var notePreviewListViewModelProvider = App.Services.GetRequiredService<NotePreviewListViewModelProvider>();

      ViewModelLease = navigationViewModelProvider.Resolve(Navigation);
      NotePreviewListViewModelLease = await notePreviewListViewModelProvider.ResolveAsync(Navigation);
    }
  }

  protected override async void OnNavigatedFrom(NavigationEventArgs e)
  {
    ViewModelLease?.Dispose();
    if (NotePreviewListViewModelLease is not null)
    {
      await NotePreviewListViewModelLease.DisposeAsync();
    }
  }

  private async void SearchResultsPage_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private void SearchResultsPage_Unloaded(object sender, RoutedEventArgs e)
  {
    Bindings.StopTracking();
  }
  #endregion
}
