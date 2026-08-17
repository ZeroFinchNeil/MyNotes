using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Navigations;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Navigations.Providers;
using MyNotes.ViewModels.Notes;
using MyNotes.ViewModels.Notes.Providers;

namespace MyNotes.Views.Navigations;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class UserListPage : Page
{
  private IViewModelLease<NavigationViewModelBase>? ViewModelLease;
  private UserListNavigationViewModel? ViewModel => ViewModelLease?.ViewModel as UserListNavigationViewModel;

  private NoteListViewModelProvider? NoteListViewModelProvider;

  private IAsyncViewModelLease<NoteListViewModel>? NoteListViewModelLease;
  private NoteListViewModel? NoteListViewModel => NoteListViewModelLease?.ViewModel;

  #region Object Lifetime Management
  public UserListPage()
  {
    TrackReference();
    InitializeComponent();

    this.Loaded += UserListPage_Loaded;
    this.Unloaded += UserListPage_Unloaded;
  }

  // OnNavigatedTo -> Loaded, OnNavigatedFrom -> Unloaded

  protected override async void OnNavigatedTo(NavigationEventArgs e)
  {
    if (e.Parameter is NavigationUserLeafNode navigation)
    {
      var navigationViewModelProvider = App.Services.GetRequiredService<NavigationViewModelProvider>();
      NoteListViewModelProvider = App.Services.GetRequiredService<NoteListViewModelProvider>();
      ViewModelLease = navigationViewModelProvider.Resolve(navigation);
      NoteListViewModelLease = await NoteListViewModelProvider.ResolveAsync(navigation);
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

  private async void UserListPage_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private void UserListPage_Unloaded(object sender, RoutedEventArgs e)
  {
    Bindings.StopTracking();
  }
  #endregion
}
