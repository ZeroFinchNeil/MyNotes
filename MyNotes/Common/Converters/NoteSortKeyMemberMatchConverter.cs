using Microsoft.UI.Xaml.Data;

using MyNotes.Application.Contracts.Notes.Models;

namespace MyNotes.Common.Converters;

internal sealed partial class NoteSortKeyMemberMatchConverter : IValueConverter
{
  public NoteSortKey TargetMember { get; set; }

  public object Convert(object value, Type targetType, object parameter, string language)
  {
    if (!targetType.Equals(typeof(bool)))
    {
      throw new ArgumentException($"Target type is not a type of {nameof(Boolean)}", nameof(targetType));
    }

    if (value is not NoteSortKey sourceValue)
    {
      throw new ArgumentException($"Value is not a type of {nameof(NoteSortKey)}", nameof(value));
    }

    return sourceValue == TargetMember;
  }

  public object ConvertBack(object value, Type targetType, object parameter, string language)
  {
    if (!targetType.Equals(typeof(NoteSortKey)))
    {
      throw new ArgumentException($"Target type is not a type of {nameof(NoteSortKey)}", nameof(targetType));
    }

    if (value is not bool targetValue)
    {
      throw new ArgumentException($"Value is not a type of {nameof(Boolean)}", nameof(value));
    }

    return targetValue ? TargetMember : -1;
  }
}