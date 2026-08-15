using Microsoft.Extensions.DependencyInjection;

using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Querying.Models;
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
      NoteListViewModel?.ChangePreviewLayout(UserListPage_NotesListGridView);
    }
  }

  private async void UserListPage_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private async void UserListPage_Unloaded(object sender, RoutedEventArgs e)
  {
    ViewModelLease?.Dispose();
    if (NoteListViewModelLease is not null)
    {
      await NoteListViewModelLease.DisposeAsync();
    }
  }
  #endregion

  private void UserListPage_MoreButtonMenuFlyout_Opening(object sender, object e)
  {

  }

  private void UserListPage_NoteSortKeyRadioMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    if (sender is RadioMenuFlyoutItem item)
    {
      NoteListViewModel?.NoteSortKey = item.Tag switch
      {
        int intValue => (NoteSortKey)intValue,
        NoteSortKey noteSortKey => noteSortKey,
        _ => throw new ArgumentException("Type mismatch")
      };
    }
  }

  private void UserListPage_NoteSortDirectionRadioMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    if (sender is RadioMenuFlyoutItem item)
    {
      NoteListViewModel?.NoteSortDirection = item.Tag switch
      {
        int intValue => (SortDirection)intValue,
        SortDirection sortDirection => sortDirection,
        _ => throw new ArgumentException("Type mismatch")
      };
    }
  }

  // TwoWay Binding BindBack
  private PreviewLayoutType PreviewLayoutTypeBindBack(int index)
    => NoteListViewModel?.ToPreviewLayoutType(index, (type) =>
    {
      NoteListViewModel.PreviewLayoutType = type;
      NoteListViewModel.ChangePreviewLayout(UserListPage_NotesListGridView);
    }) ?? PreviewLayoutType.Grid;

  private PreviewTileSize PreviewTileSizeBindBack(double index)
  => NoteListViewModel?.ToPreviewTileSize(index, (size) =>
  {
    NoteListViewModel.PreviewTileSize = size;
    NoteListViewModel.ChangePreviewTile(UserListPage_NotesListGridView);
  }) ?? PreviewTileSize.Medium;

  private PreviewTileRatio PreviewTileRatioBindBack(double index)
  => NoteListViewModel?.ToPreviewTileRatio(index, (ratio) =>
  {
    NoteListViewModel.PreviewTileRatio = ratio;
    NoteListViewModel.ChangePreviewTile(UserListPage_NotesListGridView);
  }) ?? PreviewTileRatio.Square;
}
