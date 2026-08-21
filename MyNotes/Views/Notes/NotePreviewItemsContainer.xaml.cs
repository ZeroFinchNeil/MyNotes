using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Common.Layout;

using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyNotes.Views.Notes;

public sealed partial class NotePreviewItemsContainer : UserControl
{
  public NotePreviewItemsContainer()
  {
    InitializeComponent();
    ChangePreviewLayout();
  }


  public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(object), typeof(NotePreviewItemsContainer), new PropertyMetadata(null));
  public object? ItemsSource
  {
    get => (object?)GetValue(ItemsSourceProperty);
    set => SetValue(ItemsSourceProperty, value);
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
