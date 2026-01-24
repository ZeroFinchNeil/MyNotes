using Microsoft.Extensions.DependencyInjection;

using MyNotes.Debugging;
using MyNotes.Models.Navigations;
using MyNotes.Services.Views;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Notes;

namespace MyNotes.Views.Navigations;

internal sealed partial class SearchResultsPage : Page
{
  private SearchNavigationViewModel? ViewModel;
  private NoteListViewModel? NoteListViewModel;

  public SearchResultsPage()
  {
    InitializeComponent();

    this.Loaded += SearchResultsPage_Loaded;
    this.Unloaded += SearchResultsPage_Unloaded;
  }

  private async void SearchResultsPage_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private void SearchResultsPage_Unloaded(object sender, RoutedEventArgs e)
  {
    Bindings.StopTracking();
  }

  protected override void OnNavigatedTo(NavigationEventArgs e)
  {
    if (e.Parameter is NavigationSearch navigation)
    {
      var navigationViewModelProvider = App.Instance.Services.GetRequiredService<NavigationViewModelProvider>();
      var noteListViewModelProvider = App.Instance.Services.GetRequiredService<NoteListViewModelProvider>();
      NoteListViewModel = noteListViewModelProvider.Resolve(navigation);
      if (navigationViewModelProvider.TryResolve(navigation, out var viewmodel)
          && viewmodel is SearchNavigationViewModel searchNavigationViewModel)
      {
        ViewModel = searchNavigationViewModel;
        NoteListViewModel.ChangePreviewLayout(SearchResultsPage_NotesListGridView);
#if DEBUG
        ReferenceTracker.SearchResultsPageReference.Add(this, GetHashCode());
#endif
      }
    }
  }

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
