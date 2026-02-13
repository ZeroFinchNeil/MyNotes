using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace MyNotes.Templates;

public sealed partial class AlternateIconToggleButton : ToggleButton
{
  public AlternateIconToggleButton()
  {
    DefaultStyleKey = typeof(AlternateIconToggleButton);
  }

  public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(UIElement), typeof(AlternateIconToggleButton), new PropertyMetadata(null));
  public UIElement Icon
  {
    get => (UIElement)GetValue(IconProperty);
    set => SetValue(IconProperty, value);
  }

  public static readonly DependencyProperty CheckedIconProperty = DependencyProperty.Register("CheckedIcon", typeof(UIElement), typeof(AlternateIconToggleButton), new PropertyMetadata(null));
  public UIElement CheckedIcon
  {
    get => (UIElement)GetValue(CheckedIconProperty);
    set => SetValue(CheckedIconProperty, value);
  }

  public static readonly DependencyProperty IndeterminateIconProperty = DependencyProperty.Register("IndeterminateIcon", typeof(UIElement), typeof(AlternateIconToggleButton), new PropertyMetadata(null));
  public UIElement IndeterminateIcon
  {
    get => (UIElement)GetValue(IndeterminateIconProperty);
    set => SetValue(IndeterminateIconProperty, value);
  }

  public static readonly DependencyProperty IconViewBoxWidthProperty = DependencyProperty.Register("IconViewBoxWidth", typeof(double), typeof(AlternateIconToggleButton), new PropertyMetadata(16.0));
  public double IconViewBoxWidth
  {
    get => (double)GetValue(IconViewBoxWidthProperty);
    set => SetValue(IconViewBoxWidthProperty, value);
  }

  public static readonly DependencyProperty IconViewBoxHeightProperty = DependencyProperty.Register("IconViewBoxHeight", typeof(double), typeof(AlternateIconToggleButton), new PropertyMetadata(16.0));
  public double IconViewBoxHeight
  {
    get => (double)GetValue(IconViewBoxHeightProperty);
    set => SetValue(IconViewBoxHeightProperty, value);
  }
}
