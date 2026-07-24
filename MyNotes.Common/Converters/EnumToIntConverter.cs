using System;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace MyNotes.Common.Converters;

public sealed partial class EnumToIntConverter : DependencyObject, IValueConverter
{
  public static readonly DependencyProperty EnumTypeProperty = DependencyProperty.Register("EnumType", typeof(Type), typeof(EnumToIntConverter), new PropertyMetadata(typeof(Enum)));
  public Type EnumType
  {
    get => (Type)GetValue(EnumTypeProperty);
    set => SetValue(EnumTypeProperty, value);
  }

  public object Convert(object value, Type targetType, object parameter, string language) => value.GetType().Equals(EnumType) && value is Enum enumValue
      ? (int)(ValueType)enumValue
      : throw new ArgumentException($"Value is not a type of {EnumType}");

  public object ConvertBack(object value, Type targetType, object parameter, string language) => value is int intValue
    ? Enum.ToObject(EnumType, intValue)
    : throw new ArgumentException("Value is not a type of int");
}
