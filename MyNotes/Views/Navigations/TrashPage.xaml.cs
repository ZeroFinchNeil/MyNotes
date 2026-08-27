using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Navigations.Core;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Navigations.Contents;
using MyNotes.ViewModels.Navigations.Contents.Providers;
using MyNotes.ViewModels.Navigations.Items;
using MyNotes.ViewModels.Navigations.Items.Providers;
using MyNotes.ViewModels.Notes;

namespace MyNotes.Views.Navigations;

[Debugging.Attributes.ReferenceTracker]
public sealed partial class TrashPage : Page
{
  private NavigationTrash? Navigation;

  private IViewModelLease<NavigationViewModelBase>? ViewModelLease;
  private IAsyncViewModelLease<NavigationNoteListViewModel>? NotePreviewListViewModelLease;

  private CoreNavigationViewModel? ViewModel => ViewModelLease?.ViewModel as CoreNavigationViewModel;
  private NavigationNoteListViewModel? NotePreviewListViewModel => NotePreviewListViewModelLease?.ViewModel;

  #region Object Lifetime Management
  public TrashPage()
  {
    TrackReference();
    InitializeComponent();
    this.Loaded += TrashPage_Loaded;
    this.Unloaded += TrashPage_Unloaded;
  }

  protected override async void OnNavigatedTo(NavigationEventArgs e)
  {
    if (e.Parameter is NavigationTrash navigation)
    {
      Navigation = navigation;
      var navigationViewModelProvider = App.Services.GetRequiredService<NavigationViewModelProvider>();
      var notePreviewListViewModelProvider = App.Services.GetRequiredService<NavigationNoteListViewModelProvider>();

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

  private async void TrashPage_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private void TrashPage_Unloaded(object sender, RoutedEventArgs e)
  {
    Bindings.StopTracking();
  }
  #endregion
}