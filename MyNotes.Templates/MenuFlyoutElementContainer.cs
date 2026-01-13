using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace MyNotes.Templates;

[ContentProperty(Name = "Content")]
public sealed partial class MenuFlyoutElementContainer : MenuFlyoutItem
{
  public MenuFlyoutElementContainer()
  {
    DefaultStyleKey = typeof(MenuFlyoutElementContainer);
  }

  public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(MenuFlyoutElementContainer), new PropertyMetadata(null));
  public object Content
  {
    get => GetValue(ContentProperty);
    set => SetValue(ContentProperty, value);
  }
}
