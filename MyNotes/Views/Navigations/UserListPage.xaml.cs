using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Structures;
using MyNotes.Debugging;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.ViewModels.Navigations;

namespace MyNotes.Views.Navigations;

public sealed partial class UserListPage : Page
{
  private UserLeafNavigationViewModel? ViewModel;

  public UserListPage()
  {
    InitializeComponent();
    this.Loaded += UserListPage_Loaded;
  }

  protected override async void OnNavigatedTo(NavigationEventArgs e)
  {
    if (e.Parameter is NavigationUserLeafNode navigation)
    {
      var provider = App.Instance.Services.GetRequiredService<NavigationViewModelProvider>();
      ViewModel = provider.Resolve(navigation) as UserLeafNavigationViewModel;
      if (ViewModel is not null)
      {
        ChangePreviewLayout();
        await ViewModel.LoadNoteViewModels();
        this.Unloaded += UserListPage_Unloaded;
      }

#if DEBUG
      ReferenceTracker.UserListPageReference.Add(this, ViewModel?.GetHashCode());
#endif
    }
  }

  private void UserListPage_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private void UserListPage_Unloaded(object sender, RoutedEventArgs e)
  {
    ViewModel?.UnloadNoteViewModels();
    Bindings.StopTracking();
  }

  private void UserListPage_MoreButtonMenuFlyout_Opening(object sender, object e)
  {

  }

  private void UserListPage_NoteSortKeyRadioMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    if (sender is RadioMenuFlyoutItem item)
    {
      ViewModel?.Navigation.NoteSortKey = item.Tag switch
      {
        int intValue => (NoteSortKey)intValue,
        NoteSortKey noteSortKey => noteSortKey,
        _ => throw new ArgumentException("Type mismatch")
      };
      UserListPage_NotesListGridView.UpdateLayout();
    }
  }

  private void UserListPage_NoteSortDirectionRadioMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    if (sender is RadioMenuFlyoutItem item)
    {
      ViewModel?.Navigation.NoteSortDirection = item.Tag switch
      {
        int intValue => (SortDirection)intValue,
        SortDirection sortDirection => sortDirection,
        _ => throw new ArgumentException("Type mismatch")
      };
      UserListPage_NotesListGridView.UpdateLayout();
    }
  }

  private bool Equals(NoteSortKey key1, NoteSortKey key2) => key1 == key2;
  private bool Equals(SortDirection key1, SortDirection key2) => key1 == key2;
  private Visibility VisibleWhenEquals(PreviewLayoutType key1, PreviewLayoutType key2) => key1 == key2 ? Visibility.Visible : Visibility.Collapsed;

  private static readonly BijectiveMap<PreviewLayoutType, int> _previewLayoutTypeMap = new()
  {
    { PreviewLayoutType.Grid, (int)PreviewLayoutType.Grid },
    { PreviewLayoutType.List, (int)PreviewLayoutType.List },
  };
  private IReadOnlyBijectiveMap<PreviewLayoutType, int> PreviewLayoutTypeMap => _previewLayoutTypeMap;

  private PreviewLayoutType SelectedIndexToPreviewLayoutType(int index)
  {
    var previewLayoutType = PreviewLayoutTypeMap.LeftFromRight(index);
    ViewModel?.Navigation.PreviewLayoutType = previewLayoutType;
    ChangePreviewLayout();
    return previewLayoutType;
  }

  private void ChangePreviewLayout()
  {
    PreviewLayoutType layoutType = ViewModel?.Navigation.PreviewLayoutType ?? PreviewLayoutType.Grid;
    if (layoutType is PreviewLayoutType.Grid)
    {
      UserListPage_NotesListGridView.ItemsPanel = Resources["UserListPage_GridViewItemsPanel_LayoutGrid"] as ItemsPanelTemplate;
      UserListPage_NotesListGridView.ItemTemplate = Resources["UserListPage_GridViewItemTemplate_LayoutGrid"] as DataTemplate;
    }
    else if (layoutType is PreviewLayoutType.List)
    {
      UserListPage_NotesListGridView.ItemsPanel = Resources["UserListPage_GridViewItemsPanel_LayoutList"] as ItemsPanelTemplate;
      UserListPage_NotesListGridView.ItemTemplate = Resources["UserListPage_GridViewItemTemplate_LayoutList"] as DataTemplate;
    }
    ChangePreviewTile();
  }

  private static readonly BijectiveMap<PreviewTileSize, double> _previewTileSizeMap = new()
  {
    { PreviewTileSize.Smallest, 120 },
    { PreviewTileSize.Smaller, 160 },
    { PreviewTileSize.Small, 200 },
    { PreviewTileSize.Medium, 240 },
    { PreviewTileSize.Large, 280 },
    { PreviewTileSize.Larger, 320 },
    { PreviewTileSize.Largest, 360 },
  };
  private IReadOnlyBijectiveMap<PreviewTileSize, double> PreviewTileSizeMap => _previewTileSizeMap;

  private PreviewTileSize SliderValueToPreviewTileSize(double value)
  {
    var previewTileSize = PreviewTileSizeMap.LeftFromRight(value);
    ViewModel?.Navigation.PreviewTileSize = previewTileSize;
    ChangePreviewTile();
    return previewTileSize;
  }

  private readonly BijectiveMap<PreviewTileRatio, double> _previewTileRatioMap = new()
  {
    { PreviewTileRatio.Shorter, 0.50 },
    { PreviewTileRatio.Short, 0.75 },
    { PreviewTileRatio.Square, 1.00 },
    { PreviewTileRatio.Tall, 1.25 },
    { PreviewTileRatio.Taller, 1.50 },
  };
  private IReadOnlyBijectiveMap<PreviewTileRatio, double> PreviewTileRatioMap => _previewTileRatioMap;

  private PreviewTileRatio SliderValueToPreviewTileRatio(double value)
  {
    var previewTileRatio = PreviewTileRatioMap.LeftFromRight(value);
    ViewModel?.Navigation.PreviewTileRatio = previewTileRatio;
    ChangePreviewTile();
    return previewTileRatio;
  }

  private void ChangePreviewTile()
  {
    PreviewTileSize tileSize = ViewModel?.Navigation.PreviewTileSize ?? PreviewTileSize.Medium;
    PreviewTileRatio tileRatio = ViewModel?.Navigation.PreviewTileRatio ?? PreviewTileRatio.Square;
    PreviewLayoutType layoutType = ViewModel?.Navigation.PreviewLayoutType ?? PreviewLayoutType.Grid;

    var size = PreviewTileSizeMap.RightFromLeft(tileSize);
    var ratio = PreviewTileRatioMap.RightFromLeft(tileRatio);

    if (Resources["UserListPage_GridViewItemContainerStyle"] is Style defaultStyle)
    {
      Style style = new() { TargetType = typeof(GridViewItem), BasedOn = defaultStyle };
      if (layoutType is PreviewLayoutType.Grid)
      {
        style.Setters.Add(new Setter() { Property = WidthProperty, Value = size });
        style.Setters.Add(new Setter() { Property = HeightProperty, Value = size * ratio });
      }
      else if (layoutType is PreviewLayoutType.List)
      {
        style.Setters.Add(new Setter() { Property = HeightProperty, Value = size * 0.625 });
      }

      UserListPage_NotesListGridView.ItemContainerStyle = style;
    }
  }
}
