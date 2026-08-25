using Microsoft.UI.Xaml.Data;

namespace MyNotes.Common.Converters;

internal sealed partial class RangeConverter : IValueConverter
{
  public double SourceMinimum { get; set; } = 0.0;

  public double SourceMaximum { get; set; } = 1.0;

  private double SourceRange => SourceMaximum - SourceMinimum;

  public double TargetMinimum { get; set; } = 0.0;

  public double TargetMaximum { get; set; } = 100.0;

  private double TargetRange => TargetMaximum - TargetMinimum;

  public object Convert(object value, Type targetType, object parameter, string language)
  {
    if (value is not double doubleValue)
    {
      throw new InvalidOperationException("값은 double이어야 합니다.");
    }
    var ratio = (Math.Clamp(doubleValue, SourceMinimum, SourceMaximum) - SourceMinimum) / SourceRange;
    return TargetMinimum + (ratio * TargetRange);
  }

  public object ConvertBack(object value, Type targetType, object parameter, string language)
  {
    if (value is not double doubleValue)
    {
      throw new InvalidOperationException("값은 double이어야 합니다.");
    }
    var ratio = (Math.Clamp(doubleValue, TargetMinimum, TargetMaximum) - TargetMinimum) / TargetRange;
    return SourceMinimum + (ratio * SourceRange);
  }
}