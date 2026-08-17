using Microsoft.UI.Xaml.Data;

using MyNotes.Application.Contracts.Querying.Models;

namespace MyNotes.Common.Converters;

internal sealed partial class SortDirectionMemberMatchConverter : IValueConverter
{
  public SortDirection TargetMember { get; set; }

  public object Convert(object value, Type targetType, object parameter, string language)
  {
    if (!targetType.Equals(typeof(bool)))
    {
      throw new ArgumentException($"Target type is not a type of {nameof(Boolean)}", nameof(targetType));
    }

    if (value is not SortDirection sourceValue)
    {
      throw new ArgumentException($"Value is not a type of {nameof(SortDirection)}", nameof(value));
    }

    return sourceValue == TargetMember;
  }

  public object ConvertBack(object value, Type targetType, object parameter, string language)
  {
    if (!targetType.Equals(typeof(SortDirection)))
    {
      throw new ArgumentException($"Target type is not a type of {nameof(SortDirection)}", nameof(targetType));
    }

    if (value is not bool targetValue)
    {
      throw new ArgumentException($"Value is not a type of {nameof(Boolean)}", nameof(value));
    }

    return targetValue ? TargetMember : -1;
  }
}