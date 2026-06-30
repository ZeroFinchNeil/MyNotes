using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Navigations;
using MyNotes.Shared.Enums.Navigations;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Navigations.Providers;
using MyNotes.ViewModels.Notes;
using MyNotes.ViewModels.Notes.Providers;

namespace MyNotes.Views.Navigations;

[Debugging.Attributes.ReferenceTracker]
public sealed partial class TrashPage : Page
{
  private CoreNavigationViewModel? ViewModel;
  private NoteListViewModelProvider? NoteListViewModelProvider;
  private NoteListViewModel? NoteListViewModel;

  #region Object Lifetime Management
  public TrashPage()
  {
    TrackReference();
    InitializeComponent();
    this.Loaded += TrashPage_Loaded;
    this.Unloaded += TrashPage_Unloaded;
  }

  protected override void OnNavigatedTo(NavigationEventArgs e)
  {
    if (e.Parameter is NavigationTrash navigation)
    {
      var navigationViewModelProvider = App.Services.GetRequiredService<NavigationViewModelProvider>();
      NoteListViewModelProvider = App.Services.GetRequiredService<NoteListViewModelProvider>();
      NoteListViewModel = NoteListViewModelProvider.Resolve(navigation);
      if (navigationViewModelProvider.TryResolve(navigation, out var viewmodel)
          && viewmodel is CoreNavigationViewModel trashViewModel)
      {
        ViewModel = trashViewModel;
        NoteListViewModel.ChangePreviewLayout(TrashPage_NotesListGridView);
      }
    }
  }
  protected override void OnNavigatedFrom(NavigationEventArgs e)
  {
    if (ViewModel?.Navigation is NavigationTrash navigation)
    {
      NoteListViewModelProvider?.Release(navigation);
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

  // TwoWay Binding BindBack
  private PreviewLayoutType PreviewLayoutTypeBindBack(int index)
    => NoteListViewModel?.ToPreviewLayoutType(index, (type) =>
    {
      NoteListViewModel.PreviewLayoutType = type;
      NoteListViewModel.ChangePreviewLayout(TrashPage_NotesListGridView);
    }) ?? PreviewLayoutType.Grid;

  private PreviewTileSize PreviewTileSizeBindBack(double index)
  => NoteListViewModel?.ToPreviewTileSize(index, (size) =>
  {
    NoteListViewModel.PreviewTileSize = size;
    NoteListViewModel.ChangePreviewTile(TrashPage_NotesListGridView);
  }) ?? PreviewTileSize.Medium;

  private PreviewTileRatio PreviewTileRatioBindBack(double index)
  => NoteListViewModel?.ToPreviewTileRatio(index, (ratio) =>
  {
    NoteListViewModel.PreviewTileRatio = ratio;
    NoteListViewModel.ChangePreviewTile(TrashPage_NotesListGridView);
  }) ?? PreviewTileRatio.Square;
}