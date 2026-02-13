using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyNotes.Templates;

public sealed partial class SubtleIconLabelButton : Button
{
  public SubtleIconLabelButton()
  {
    DefaultStyleKey = typeof(SubtleIconLabelButton);
  }

  public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(UIElement), typeof(SubtleIconLabelButton), new PropertyMetadata(null));
  public UIElement Icon
  {
    get => (UIElement)GetValue(IconProperty);
    set => SetValue(IconProperty, value);
  }

  public static readonly DependencyProperty IconViewBoxWidthProperty = DependencyProperty.Register("IconViewBoxWidth", typeof(double), typeof(SubtleIconLabelButton), new PropertyMetadata(16.0));
  public double IconViewBoxWidth
  {
    get => (double)GetValue(IconViewBoxWidthProperty);
    set => SetValue(IconViewBoxWidthProperty, value);
  }

  public static readonly DependencyProperty IconViewBoxHeightProperty = DependencyProperty.Register("IconViewBoxHeight", typeof(double), typeof(SubtleIconLabelButton), new PropertyMetadata(16.0));
  public double IconViewBoxHeight
  {
    get => (double)GetValue(IconViewBoxHeightProperty);
    set => SetValue(IconViewBoxHeightProperty, value);
  }
}
