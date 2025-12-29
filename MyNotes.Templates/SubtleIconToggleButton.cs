using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace MyNotes.Templates;

public sealed partial class SubtleIconToggleButton : ToggleButton
{
  public SubtleIconToggleButton()
  {
    DefaultStyleKey = typeof(SubtleIconToggleButton);
  }

  public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(IconElement), typeof(SubtleIconToggleButton), new PropertyMetadata(null));
  public IconElement Icon
  {
    get => (IconElement)GetValue(IconProperty);
    set => SetValue(IconProperty, value);
  }
}
