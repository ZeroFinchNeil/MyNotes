using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyNotes.Templates;

public sealed partial class SubtleIconButton : Button
{
  public SubtleIconButton()
  {
    DefaultStyleKey = typeof(SubtleIconButton);
  }

  public static readonly DependencyProperty IconWidthProperty = DependencyProperty.Register("IconWidth", typeof(double), typeof(SubtleIconButton), new PropertyMetadata(null));
  public double IconWidth
  {
    get => (double)GetValue(IconWidthProperty);
    set => SetValue(IconWidthProperty, value);
  }

  public static readonly DependencyProperty IconHeightProperty = DependencyProperty.Register("IconHeight", typeof(double), typeof(SubtleIconButton), new PropertyMetadata(null));
  public double IconHeight
  {
    get => (double)GetValue(IconHeightProperty);
    set => SetValue(IconHeightProperty, value);
  }
}
