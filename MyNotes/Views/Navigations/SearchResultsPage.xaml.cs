using Microsoft.Extensions.DependencyInjection;

using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Models.Navigations;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Navigations.Providers;
using MyNotes.ViewModels.Notes;
using MyNotes.ViewModels.Notes.Providers;

namespace MyNotes.Views.Navigations;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class SearchResultsPage : Page
{
  private SearchNavigationViewModel? ViewModel;
  private NoteListViewModelProvider? NoteListViewModelProvider;
  private NoteListViewModel? NoteListViewModel;

  #region Object Lifetime Management
  public SearchResultsPage()
  {
    TrackReference();
    InitializeComponent();

    this.Loaded += SearchResultsPage_Loaded;
    this.Unloaded += SearchResultsPage_Unloaded;
  }

  protected override void OnNavigatedTo(NavigationEventArgs e)
  {
    if (e.Parameter is NavigationSearch navigation)
    {
      var navigationViewModelProvider = App.Services.GetRequiredService<NavigationViewModelProvider>();
      NoteListViewModelProvider = App.Services.GetRequiredService<NoteListViewModelProvider>();
      NoteListViewModel = NoteListViewModelProvider.Resolve(navigation);
      if (navigationViewModelProvider.TryResolve(navigation, out var viewmodel)
          && viewmodel is SearchNavigationViewModel searchNavigationViewModel)
      {
        ViewModel = searchNavigationViewModel;
        NoteListViewModel.ChangePreviewLayout(SearchResultsPage_NotesListGridView);
      }
    }
  }

  protected override void OnNavigatedFrom(NavigationEventArgs e)
  {
    if (ViewModel?.Navigation is NavigationSearch navigation)
    {
      NoteListViewModelProvider?.Release(navigation);
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

  // TwoWay Binding BindBack
  private PreviewLayoutType PreviewLayoutTypeBindBack(int index)
    => NoteListViewModel?.ToPreviewLayoutType(index, (type) =>
    {
      NoteListViewModel.PreviewLayoutType = type;
      NoteListViewModel.ChangePreviewLayout(SearchResultsPage_NotesListGridView);
    }) ?? PreviewLayoutType.Grid;

  private PreviewTileSize PreviewTileSizeBindBack(double index)
  => NoteListViewModel?.ToPreviewTileSize(index, (size) =>
  {
    NoteListViewModel.PreviewTileSize = size;
    NoteListViewModel.ChangePreviewTile(SearchResultsPage_NotesListGridView);
  }) ?? PreviewTileSize.Medium;

  private PreviewTileRatio PreviewTileRatioBindBack(double index)
  => NoteListViewModel?.ToPreviewTileRatio(index, (ratio) =>
  {
    NoteListViewModel.PreviewTileRatio = ratio;
    NoteListViewModel.ChangePreviewTile(SearchResultsPage_NotesListGridView);
  }) ?? PreviewTileRatio.Square;
}
