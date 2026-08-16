using Microsoft.UI.Xaml.Data;

using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Common.Layout;

namespace MyNotes.Common.Converters;

internal sealed partial class PreviewTileSizeToSliderValueConverter : DependencyObject, IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, string language) => value is PreviewTileSize previewTileSize
    ? PreviewTileSizeMetrics.GetWidth(previewTileSize)
    : throw new ArgumentException($"Value is not a type of {nameof(PreviewTileSize)}", nameof(value));

  public object ConvertBack(object value, Type targetType, object parameter, string language) => value is double doubleValue
    ? PreviewTileSizeMetrics.GetWidth(doubleValue)
    : throw new ArgumentException("Value is not a type of double", nameof(value));
}