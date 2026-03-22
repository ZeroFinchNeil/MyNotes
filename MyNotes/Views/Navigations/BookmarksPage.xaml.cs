using Microsoft.Extensions.DependencyInjection;

using MyNotes.Debugging;
using MyNotes.Models.Navigations;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Navigations.Providers;
using MyNotes.ViewModels.Notes;
using MyNotes.ViewModels.Notes.Providers;

namespace MyNotes.Views.Navigations;

public sealed partial class BookmarksPage : Page
{
  private CoreNavigationViewModel? ViewModel;
  private NoteListViewModel? NoteListViewModel;

  #region Object Lifetime Management
  public BookmarksPage()
  {
#if DEBUG
    if (Debugger.IsAttached)
    {
      ReferenceTracker.PageReference.Add(this, $"{GetType().Name}: {GetHashCode()}");
    }
#endif

    InitializeComponent();
    this.Loaded += BookmarksPage_Loaded;
    this.Unloaded += BookmarksPage_Unloaded;
  }

  protected override void OnNavigatedTo(NavigationEventArgs e)
  {
    if (e.Parameter is NavigationBookmarks navigation)
    {
      var navigationViewModelProvider = App.Services.GetRequiredService<NavigationViewModelProvider>();
      var noteListViewModelProvider = App.Services.GetRequiredService<NoteListViewModelProvider>();
      NoteListViewModel = noteListViewModelProvider.Resolve(navigation);
      if (navigationViewModelProvider.TryResolve(navigation, out var viewmodel)
          && viewmodel is CoreNavigationViewModel bookmarksViewModel)
      {
        ViewModel = bookmarksViewModel;
        NoteListViewModel.ChangePreviewLayout(BookmarksPage_NotesListGridView);
      }
    }
  }
  protected override void OnNavigatedFrom(NavigationEventArgs e)
  {
    NoteListViewModel?.Dispose();
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

  // TwoWay Binding BindBack
  private PreviewLayoutType PreviewLayoutTypeBindBack(int index)
    => NoteListViewModel?.ToPreviewLayoutType(index, (type) =>
    {
      NoteListViewModel.PreviewLayoutType = type;
      NoteListViewModel.ChangePreviewLayout(BookmarksPage_NotesListGridView);
    }) ?? PreviewLayoutType.Grid;

  private PreviewTileSize PreviewTileSizeBindBack(double index)
  => NoteListViewModel?.ToPreviewTileSize(index, (size) =>
  {
    NoteListViewModel.PreviewTileSize = size;
    NoteListViewModel.ChangePreviewTile(BookmarksPage_NotesListGridView);
  }) ?? PreviewTileSize.Medium;

  private PreviewTileRatio PreviewTileRatioBindBack(double index)
  => NoteListViewModel?.ToPreviewTileRatio(index, (ratio) =>
  {
    NoteListViewModel.PreviewTileRatio = ratio;
    NoteListViewModel.ChangePreviewTile(BookmarksPage_NotesListGridView);
  }) ?? PreviewTileRatio.Square;
}
