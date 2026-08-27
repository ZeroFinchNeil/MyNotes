using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Navigations;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Navigations.Contents;
using MyNotes.ViewModels.Navigations.Contents.Providers;
using MyNotes.ViewModels.Navigations.Items;
using MyNotes.ViewModels.Navigations.Items.Providers;

namespace MyNotes.Views.Navigations;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class SearchResultsPage : Page
{
  private NavigationSearch? Navigation;
  private IAsyncViewModelLease<NavigationNoteListViewModel>? NotePreviewListViewModelLease;

  private NavigationNoteListViewModel? NotePreviewListViewModel => NotePreviewListViewModelLease?.ViewModel;

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

      var notePreviewListViewModelProvider = App.Services.GetRequiredService<NavigationNoteListViewModelProvider>();
      NotePreviewListViewModelLease = await notePreviewListViewModelProvider.ResolveAsync(Navigation);
    }
  }

  protected override async void OnNavigatedFrom(NavigationEventArgs e)
  {
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
