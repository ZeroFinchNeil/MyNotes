using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Navigations;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Navigations.Providers;
using MyNotes.ViewModels.Notes;
using MyNotes.ViewModels.Notes.Providers;

namespace MyNotes.Views.Navigations;

[Debugging.Attributes.ReferenceTracker]
public sealed partial class BookmarksPage : Page
{
  private IViewModelLease<NavigationViewModelBase>? ViewModelLease;
  private CoreNavigationViewModel? ViewModel => ViewModelLease?.ViewModel as CoreNavigationViewModel;
  private NavigationBookmarks? Navigation => ViewModel?.Navigation as NavigationBookmarks;
  private IAsyncViewModelLease<NoteListViewModel>? NoteListViewModelLease;
  private NoteListViewModel? NoteListViewModel => NoteListViewModelLease?.ViewModel;

  #region Object Lifetime Management
  public BookmarksPage()
  {
    TrackReference();
    InitializeComponent();
    this.Loaded += BookmarksPage_Loaded;
    this.Unloaded += BookmarksPage_Unloaded;
  }

  protected override async void OnNavigatedTo(NavigationEventArgs e)
  {
    if (e.Parameter is NavigationBookmarks navigation)
    {
      var navigationViewModelProvider = App.Services.GetRequiredService<NavigationViewModelProvider>();
      ViewModelLease = navigationViewModelProvider.Acquire(navigation);

      var noteListViewModelProvider = App.Services.GetRequiredService<NoteListViewModelProvider>();
      NoteListViewModelLease = await noteListViewModelProvider.ResolveAsync(navigation);
    }
  }

  protected override async void OnNavigatedFrom(NavigationEventArgs e)
  {
    ViewModelLease?.Dispose();
    if (NoteListViewModelLease is not null)
    {
      await NoteListViewModelLease.DisposeAsync();
    }
  }

  private async void BookmarksPage_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private void BookmarksPage_Unloaded(object sender, RoutedEventArgs e)
  {
    Bindings.StopTracking();
  }
  #endregion
}
