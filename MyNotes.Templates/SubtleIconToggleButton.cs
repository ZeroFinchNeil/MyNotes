using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Markup;

namespace MyNotes.Templates;

public sealed partial class SubtleIconToggleButton : ToggleButton
{
  public SubtleIconToggleButton()
  {
    DefaultStyleKey = typeof(SubtleIconToggleButton);
  }

  public static readonly DependencyProperty IconViewBoxWidthProperty = DependencyProperty.Register("IconViewBoxWidth", typeof(double), typeof(SubtleIconToggleButton), new PropertyMetadata(16.0));
  public double IconViewBoxWidth
  {
    get => (double)GetValue(IconViewBoxWidthProperty);
    set => SetValue(IconViewBoxWidthProperty, value);
  }

  public static readonly DependencyProperty IconViewBoxHeightProperty = DependencyProperty.Register("IconViewBoxHeight", typeof(double), typeof(SubtleIconToggleButton), new PropertyMetadata(16.0));
  public double IconViewBoxHeight
  {
    get => (double)GetValue(IconViewBoxHeightProperty);
    set => SetValue(IconViewBoxHeightProperty, value);
  }
}
