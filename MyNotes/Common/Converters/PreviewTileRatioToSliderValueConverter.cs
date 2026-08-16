using Microsoft.UI.Xaml.Data;

using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Common.Layout;

namespace MyNotes.Common.Converters;

internal sealed partial class PreviewTileRatioToSliderValueConverter : DependencyObject, IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, string language) => value is PreviewTileRatio previewTileRatio
    ? PreviewTileRatioMetrics.GetRatio(previewTileRatio)
    : throw new ArgumentException($"Value is not a type of {nameof(PreviewTileRatio)}", nameof(value));

  public object ConvertBack(object value, Type targetType, object parameter, string language) => value is double doubleValue
    ? PreviewTileRatioMetrics.FromRatio(doubleValue)
    : throw new ArgumentException("Value is not a type of double", nameof(value));
}