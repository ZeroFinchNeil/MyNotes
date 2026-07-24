using Microsoft.UI.Xaml;

namespace MyNotes.Common.Helpers;

public static class BooleanHelper
{
  public static bool Negate(bool value) => !value;

  public static Visibility ToInverseVisibility(bool value) => value ? Visibility.Collapsed : Visibility.Visible; 
}