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
  private UserListNavigationViewModel ViewModel => ViewModelLease?.ViewModel as UserListNavigationViewModel ?? throw new InvalidOperationException("NavigationViewModelLease 초기화되지 않음");
  private NavigationUserLeafNode Navigation => ViewModel.Navigation as NavigationUserLeafNode ?? throw new InvalidOperationException("Navigation 타입이 일치하지 않음");

  private IAsyncViewModelLease<NotePreviewListViewModel>? NotePreviewListViewModelLease;
  private NotePreviewListViewModel? NotePreviewListViewModel => NotePreviewListViewModelLease?.ViewModel;

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
      var notePreviewListViewModelProvider = App.Services.GetRequiredService<NotePreviewListViewModelProvider>();
      ViewModelLease = navigationViewModelProvider.Resolve(navigation);
      NotePreviewListViewModelLease = await notePreviewListViewModelProvider.ResolveAsync(navigation);
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
