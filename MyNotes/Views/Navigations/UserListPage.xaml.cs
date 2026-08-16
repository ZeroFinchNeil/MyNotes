using Microsoft.Extensions.DependencyInjection;

using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Querying.Models;
using MyNotes.Common.Layout;
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

  private void Navigation_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (ViewModel is null)
    {
      return;
    }
    switch (e.PropertyName)
    {
      case nameof(NavigationUserLeafNode.PreviewLayoutType):
      case nameof(NavigationUserLeafNode.PreviewTileSize):
      case nameof(NavigationUserLeafNode.PreviewTileRatio):
        ChangePreviewLayout(ViewModel.Navigation.PreviewLayoutType, ViewModel.Navigation.PreviewTileSize, ViewModel.Navigation.PreviewTileRatio);
        break;
    }
  }

  private async void UserListPage_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();

    ViewModel?.Navigation.PropertyChanged += Navigation_PropertyChanged;
  }

  private async void UserListPage_Unloaded(object sender, RoutedEventArgs e)
  {
    ViewModel?.Navigation.PropertyChanged -= Navigation_PropertyChanged;

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

  public void ChangePreviewLayout(PreviewLayoutType previewLayoutType, PreviewTileSize previewTileSize, PreviewTileRatio previewTileRatio)
  {
    var gridView = UserListPage_NotesListGridView;
    if (previewLayoutType is PreviewLayoutType.Grid)
    {
      gridView.ItemsPanel = App.Instance.Resources["NoteList_GridViewItemsPanel_LayoutGrid"] as ItemsPanelTemplate;
      gridView.ItemTemplate = App.Instance.Resources["NoteList_GridViewItemTemplate_LayoutGrid"] as DataTemplate;
    }
    else if (previewLayoutType is PreviewLayoutType.List)
    {
      gridView.ItemsPanel = App.Instance.Resources["NoteList_GridViewItemsPanel_LayoutList"] as ItemsPanelTemplate;
      gridView.ItemTemplate = App.Instance.Resources["NoteList_GridViewItemTemplate_LayoutList"] as DataTemplate;
    }
    ChangePreviewTile(previewLayoutType, previewTileSize, previewTileRatio);
  }

  public void ChangePreviewTile(PreviewLayoutType previewLayoutType, PreviewTileSize previewTileSize, PreviewTileRatio previewTileRatio)
  {
    var size = PreviewTileSizeMetrics.GetWidth(previewTileSize);
    var ratio = PreviewTileRatioMetrics.GetRatio(previewTileRatio);

    if (App.Instance.Resources["NoteList_GridViewItemContainerStyle"] is Style defaultStyle)
    {
      Style style = new() { TargetType = typeof(GridViewItem), BasedOn = defaultStyle };
      if (previewLayoutType is PreviewLayoutType.Grid)
      {
        style.Setters.Add(new Setter() { Property = FrameworkElement.WidthProperty, Value = size });
        style.Setters.Add(new Setter() { Property = FrameworkElement.HeightProperty, Value = size * ratio });
      }
      else if (previewLayoutType is PreviewLayoutType.List)
      {
        style.Setters.Add(new Setter() { Property = FrameworkElement.HeightProperty, Value = size * 0.625 });
      }

      UserListPage_NotesListGridView.ItemContainerStyle = style;
    }
  }
}
