using Microsoft.Extensions.DependencyInjection;

using MyNotes.Debugging;
using MyNotes.Models.Navigations;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Notes;

namespace MyNotes.Views.Navigations;

internal sealed partial class SearchResultsPage : Page
{
  private SearchNavigationViewModel? ViewModel;
  private NoteListViewModel? NoteListViewModel;

  #region Object Lifetime Management
  public SearchResultsPage()
  {
#if DEBUG
    if (Debugger.IsAttached)
    {
      ReferenceTracker.PageReference.Add(this, $"{GetType().Name}: {GetHashCode()}");
    }
#endif

    InitializeComponent();

    this.Loaded += SearchResultsPage_Loaded;
    this.Unloaded += SearchResultsPage_Unloaded;
  }

  protected override void OnNavigatedTo(NavigationEventArgs e)
  {
    if (e.Parameter is NavigationSearch navigation)
    {
      var navigationViewModelProvider = App.Services.GetRequiredService<NavigationViewModelProvider>();
      var noteListViewModelProvider = App.Services.GetRequiredService<NoteListViewModelProvider>();
      NoteListViewModel = noteListViewModelProvider.Resolve(navigation);
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
    NoteListViewModel?.Dispose();
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
