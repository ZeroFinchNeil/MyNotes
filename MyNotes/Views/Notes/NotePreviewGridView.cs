using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Common.Layout;
using MyNotes.Debugging;

namespace MyNotes.Views.Notes;

public sealed partial class NotePreviewGridView : GridView
{
  public static readonly DependencyProperty PreviewLayoutTypeProperty = DependencyProperty.Register("PreviewLayoutType", typeof(PreviewLayoutType), typeof(NotePreviewGridView), new PropertyMetadata(PreviewLayoutType.Grid, OnPreviewLayoutPropertyChanged));
  public PreviewLayoutType PreviewLayoutType
  {
    get => (PreviewLayoutType)GetValue(PreviewLayoutTypeProperty);
    set => SetValue(PreviewLayoutTypeProperty, value);
  }

  public static readonly DependencyProperty PreviewTileSizeProperty = DependencyProperty.Register("PreviewTileSize", typeof(PreviewTileSize), typeof(NotePreviewGridView), new PropertyMetadata(PreviewTileSize.Medium, OnPreviewTilePropertyChanged));
  public PreviewTileSize PreviewTileSize
  {
    get => (PreviewTileSize)GetValue(PreviewTileSizeProperty);
    set => SetValue(PreviewTileSizeProperty, value);
  }

  public static readonly DependencyProperty PreviewTileRatioProperty = DependencyProperty.Register("PreviewTileRatio", typeof(PreviewTileRatio), typeof(NotePreviewGridView), new PropertyMetadata(PreviewTileRatio.Square, OnPreviewTilePropertyChanged));
  public PreviewTileRatio PreviewTileRatio
  {
    get => (PreviewTileRatio)GetValue(PreviewTileRatioProperty);
    set => SetValue(PreviewTileRatioProperty, value);
  }

  private static void OnPreviewLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as NotePreviewGridView)?.ChangePreviewLayout();

  private static void OnPreviewTilePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as NotePreviewGridView)?.ChangePreviewTile();

  private void ChangePreviewLayout()
  {
    switch (PreviewLayoutType)
    {
      case PreviewLayoutType.Grid:
        ItemsPanel = App.Instance.Resources["NoteList_GridViewItemsPanel_LayoutGrid"] as ItemsPanelTemplate;
        ItemTemplate = App.Instance.Resources["NoteList_GridViewItemTemplate_LayoutGrid"] as DataTemplate;
        break;
      case PreviewLayoutType.List:
        ItemsPanel = App.Instance.Resources["NoteList_GridViewItemsPanel_LayoutList"] as ItemsPanelTemplate;
        ItemTemplate = App.Instance.Resources["NoteList_GridViewItemTemplate_LayoutList"] as DataTemplate;
        break;
    }
    ChangePreviewTile();
  }

  private void ChangePreviewTile()
  {
    var size = PreviewTileSizeMetrics.GetWidth(PreviewTileSize);
    var ratio = PreviewTileRatioMetrics.GetRatio(PreviewTileRatio);

    if (App.Instance.Resources["NoteList_GridViewItemContainerStyle"] is Style defaultStyle)
    {
      Style style = new() { TargetType = typeof(GridViewItem), BasedOn = defaultStyle };
      switch (PreviewLayoutType)
      {
        case PreviewLayoutType.Grid:
          style.Setters.Add(new Setter() { Property = FrameworkElement.WidthProperty, Value = size });
          style.Setters.Add(new Setter() { Property = FrameworkElement.HeightProperty, Value = size * ratio });
          break;
        case PreviewLayoutType.List:
          style.Setters.Add(new Setter() { Property = FrameworkElement.HeightProperty, Value = size * 0.625 });
          break;
      }

      ItemContainerStyle = style;
    }
  }

  public NotePreviewGridView()
  {
    DefaultStyleKey = typeof(GridView);
  }

  protected override void OnApplyTemplate()
  {
    base.OnApplyTemplate();
    ChangePreviewLayout();
  }
}
