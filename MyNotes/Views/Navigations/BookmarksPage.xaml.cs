using Microsoft.Extensions.DependencyInjection;

using MyNotes.Debugging;
using MyNotes.Models.Navigations.Core;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Navigations.Providers;
using MyNotes.ViewModels.Notes;
using MyNotes.ViewModels.Notes.Providers;

namespace MyNotes.Views.Navigations;

[Debugging.Attributes.ReferenceTracker]
public sealed partial class BookmarksPage : Page
{
  private NavigationBookmarks? Navigation;

  private IViewModelLease<NavigationViewModelBase>? ViewModelLease;
  private IAsyncViewModelLease<NotePreviewListViewModel>? NotePreviewListViewModelLease;

  private CoreNavigationViewModel? ViewModel => ViewModelLease?.ViewModel as CoreNavigationViewModel;
  private NotePreviewListViewModel? NotePreviewListViewModel => NotePreviewListViewModelLease?.ViewModel;

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
      Navigation = navigation;
      var navigationViewModelProvider = App.Services.GetRequiredService<NavigationViewModelProvider>();
      var noteListViewModelProvider = App.Services.GetRequiredService<NotePreviewListViewModelProvider>();

      ViewModelLease = navigationViewModelProvider.Acquire(Navigation);
      NotePreviewListViewModelLease = await noteListViewModelProvider.ResolveAsync(Navigation);
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
