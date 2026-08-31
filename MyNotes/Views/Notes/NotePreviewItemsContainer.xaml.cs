using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Common.Layout;
using MyNotes.Debugging;
using MyNotes.ViewModels.Navigations.Contents;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyNotes.Views.Notes;

internal sealed partial class NotePreviewItemsContainer : UserControl
{
  public NotePreviewItemsContainer()
  {
    InitializeComponent();
  }

  protected override void OnApplyTemplate()
  {
    base.OnApplyTemplate();
    ChangePreviewLayout();
  }

  public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(NavigationNoteListViewModel), typeof(NotePreviewItemsContainer), new PropertyMetadata(null));
  public NavigationNoteListViewModel ViewModel
  {
    get => (NavigationNoteListViewModel)GetValue(ViewModelProperty);
    set => SetValue(ViewModelProperty, value);
  }

  public static readonly DependencyProperty PreviewLayoutTypeProperty = DependencyProperty.Register("PreviewLayoutType", typeof(PreviewLayoutType), typeof(NotePreviewItemsContainer), new PropertyMetadata(PreviewLayoutType.Grid, OnPreviewLayoutPropertyChanged));
  public PreviewLayoutType PreviewLayoutType
  {
    get => (PreviewLayoutType)GetValue(PreviewLayoutTypeProperty);
    set => SetValue(PreviewLayoutTypeProperty, value);
  }

  public static readonly DependencyProperty PreviewTileSizeProperty = DependencyProperty.Register("PreviewTileSize", typeof(PreviewTileSize), typeof(NotePreviewItemsContainer), new PropertyMetadata(PreviewTileSize.Medium, OnPreviewTilePropertyChanged));
  public PreviewTileSize PreviewTileSize
  {
    get => (PreviewTileSize)GetValue(PreviewTileSizeProperty);
    set => SetValue(PreviewTileSizeProperty, value);
  }

  public static readonly DependencyProperty PreviewTileRatioProperty = DependencyProperty.Register("PreviewTileRatio", typeof(PreviewTileRatio), typeof(NotePreviewItemsContainer), new PropertyMetadata(PreviewTileRatio.Square, OnPreviewTilePropertyChanged));
  public PreviewTileRatio PreviewTileRatio
  {
    get => (PreviewTileRatio)GetValue(PreviewTileRatioProperty);
    set => SetValue(PreviewTileRatioProperty, value);
  }

  private static void OnPreviewLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as NotePreviewItemsContainer)?.ChangePreviewLayout();

  private static void OnPreviewTilePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as NotePreviewItemsContainer)?.ChangePreviewTile();

  private void ChangePreviewLayout()
  {
    ConsoleHelper.WriteLine(true, "{0}: {1}", "PreviewLayoutType", PreviewLayoutType);
    switch (PreviewLayoutType)
    {
      case PreviewLayoutType.Grid:
        PreviewGridView.ItemsPanel = Resources["GridViewItemsPanel_LayoutGrid"] as ItemsPanelTemplate;
        PreviewGridView.ItemTemplate = Resources["GridViewItemTemplate_LayoutGrid"] as DataTemplate;
        break;
      case PreviewLayoutType.List:
        PreviewGridView.ItemsPanel = Resources["GridViewItemsPanel_LayoutList"] as ItemsPanelTemplate;
        PreviewGridView.ItemTemplate = Resources["GridViewItemTemplate_LayoutList"] as DataTemplate;
        break;
    }
    ChangePreviewTile();
  }

  private void ChangePreviewTile()
  {
    ConsoleHelper.WriteLine(true, "{0}: {1}", "PreviewTileSize", PreviewTileSize);
    ConsoleHelper.WriteLine(true, "{0}: {1}", "PreviewTileRatio", PreviewTileRatio);
    var size = PreviewTileSizeMetrics.GetWidth(PreviewTileSize);
    var ratio = PreviewTileRatioMetrics.GetRatio(PreviewTileRatio);

    if (Resources["GridViewItemContainerStyle"] is Style defaultStyle)
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

      PreviewGridView.ItemContainerStyle = style;
    }
  }
}

