namespace MyNotes.Helpers;

internal static class DataContextHelper
{
  public static readonly DependencyProperty DataContextProperty = DependencyProperty.RegisterAttached("DataContext", typeof(object), typeof(DataContextHelper), new PropertyMetadata(null));
  public static object GetDataContext(DependencyObject obj) => obj.GetValue(DataContextProperty);
  public static void SetDataContext(DependencyObject obj, object value) => obj.SetValue(DataContextProperty, value);
}
