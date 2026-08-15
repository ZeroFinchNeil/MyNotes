using Microsoft.Extensions.DependencyInjection;

using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Models.Navigations;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Navigations.Providers;
using MyNotes.ViewModels.Notes;
using MyNotes.ViewModels.Notes.Providers;

namespace MyNotes.Views.Navigations;

[Debugging.Attributes.ReferenceTracker]
public sealed partial class TrashPage : Page
{
  private IViewModelLease<NavigationViewModelBase>? ViewModelLease;
  private CoreNavigationViewModel? ViewModel => ViewModelLease?.ViewModel as CoreNavigationViewModel;
  private IAsyncViewModelLease<NoteListViewModel>? NoteListViewModelLease;
  private NoteListViewModel? NoteListViewModel => NoteListViewModelLease?.ViewModel;

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
      var navigationViewModelProvider = App.Services.GetRequiredService<NavigationViewModelProvider>();
      ViewModelLease = navigationViewModelProvider.Acquire(navigation);

      var noteListViewModelProvider = App.Services.GetRequiredService<NoteListViewModelProvider>();
      NoteListViewModelLease = await noteListViewModelProvider.ResolveAsync(navigation);

      if (ViewModel is not null && NoteListViewModel is not null)
      {
        NoteListViewModel.ChangePreviewLayout(TrashPage_NotesListGridView);
      }
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