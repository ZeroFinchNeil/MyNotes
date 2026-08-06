using Microsoft.UI.Xaml.Data;

using MyNotes.Application.Contracts.Navigations.Models;

namespace MyNotes.Common.Converters;

internal sealed partial class PreviewTileSizeToSliderValueConverter : DependencyObject, IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, string language) => value is PreviewTileSize previewTileSize
    ? previewTileSize switch
    {
      PreviewTileSize.Smallest => 120.0,
      PreviewTileSize.Smaller => 160.0,
      PreviewTileSize.Small => 200.0,
      PreviewTileSize.Medium => 240.0,
      PreviewTileSize.Large => 280.0,
      PreviewTileSize.Larger => 320.0,
      PreviewTileSize.Largest => 360.0,
      _ => throw new InvalidOperationException()
    }
    : throw new ArgumentException($"Value is not a type of {nameof(PreviewTileSize)}");

  public object ConvertBack(object value, Type targetType, object parameter, string language) => value is double doubleValue
    ? doubleValue switch
    {
    < 140.0 => PreviewTileSize.Smallest,
    < 180.0 => PreviewTileSize.Smaller,
    < 220.0 => PreviewTileSize.Small,
    < 260.0 => PreviewTileSize.Medium,
    < 300.0 => PreviewTileSize.Large,
    < 340.0 => PreviewTileSize.Larger,
    >= 340.0 => PreviewTileSize.Largest,
    _ => throw new InvalidOperationException()
    }
    : throw new ArgumentException("Value is not a type of double");
}