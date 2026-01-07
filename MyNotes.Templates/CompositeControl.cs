using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyNotes.Templates;

public sealed partial class CompositeControl : Control
{
  public CompositeControl()
  {
    DefaultStyleKey = typeof(CompositeControl);
  }

  public static readonly DependencyProperty PrimaryContentProperty = DependencyProperty.Register("PrimaryContent", typeof(UIElement), typeof(CompositeControl), new PropertyMetadata(null));
  public UIElement PrimaryContent
  {
    get => (UIElement)GetValue(PrimaryContentProperty);
    set => SetValue(PrimaryContentProperty, value);
  }

  public static readonly DependencyProperty SecondaryContentProperty = DependencyProperty.Register("SecondaryContent", typeof(UIElement), typeof(CompositeControl), new PropertyMetadata(null));
  public UIElement SecondaryContent
  {
    get => (UIElement)GetValue(SecondaryContentProperty);
    set => SetValue(SecondaryContentProperty, value);
  }

  public static readonly DependencyProperty TertiaryContentProperty = DependencyProperty.Register("TertiaryContent", typeof(UIElement), typeof(CompositeControl), new PropertyMetadata(null));
  public UIElement TertiaryContent
  {
    get => (UIElement)GetValue(TertiaryContentProperty);
    set => SetValue(TertiaryContentProperty, value);
  }
}
