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
  private IViewModelLease<NavigationViewModelBase>? ViewModelLease;
  private SearchNavigationViewModel ViewModel => ViewModelLease?.ViewModel as SearchNavigationViewModel ?? throw new InvalidOperationException("NavigationViewModelLease 초기화되지 않음");

  private NavigationSearch Navigation => ViewModel.Navigation as NavigationSearch ?? throw new InvalidOperationException("Navigation 타입이 일치하지 않음");

  private IAsyncViewModelLease<NotePreviewListViewModel>? NotePreviewListViewModelLease;
  private NotePreviewListViewModel NotePreviewListViewModel => NotePreviewListViewModelLease?.ViewModel ?? throw new InvalidOperationException("NotePreviewListViewModelLease가 초기화되지 않음");

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
      var navigationViewModelProvider = App.Services.GetRequiredService<NavigationViewModelProvider>();
      ViewModelLease = navigationViewModelProvider.Acquire(navigation);

      var noteListViewModelProvider = App.Services.GetRequiredService<NotePreviewListViewModelProvider>();
      NotePreviewListViewModelLease = await noteListViewModelProvider.ResolveAsync(navigation);
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
