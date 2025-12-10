using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyNotes.Templates;

public sealed partial class SubtleIconLabelButton : Button
{
  public SubtleIconLabelButton()
  {
    DefaultStyleKey = typeof(SubtleIconLabelButton);
  }

  public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(IconElement), typeof(SubtleIconLabelButton), new PropertyMetadata(null));
  public IconElement Icon
  {
    get => (IconElement)GetValue(IconProperty);
    set => SetValue(IconProperty, value);
  }
}
