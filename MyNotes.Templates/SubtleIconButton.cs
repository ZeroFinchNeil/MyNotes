using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyNotes.Templates;

public sealed partial class SubtleIconButton : Button
{
  public SubtleIconButton()
  {
    DefaultStyleKey = typeof(SubtleIconButton);
  }

  public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(IconElement), typeof(SubtleIconButton), new PropertyMetadata(null));
  public IconElement Icon
  {
    get => (IconElement)GetValue(IconProperty);
    set => SetValue(IconProperty, value);
  }
}
