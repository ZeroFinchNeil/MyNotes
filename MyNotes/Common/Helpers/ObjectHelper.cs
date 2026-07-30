namespace MyNotes.Common.Helpers;

internal static class ObjectHelper
{
  public static bool IsEqual(object obj1, object obj2) => obj1.Equals(obj2);

  public static Visibility VisibleWhenEquals(object obj1, object obj2) => obj1.Equals(obj2) ? Visibility.Visible : Visibility.Collapsed;
}