using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyNotes.Templates;

public sealed partial class AlternateAppBarToggleButton : AppBarToggleButton
{
  public static readonly DependencyProperty CheckedIconProperty = DependencyProperty.Register("CheckedIcon", typeof(IconElement), typeof(AlternateAppBarToggleButton), new PropertyMetadata(null));
  public IconElement CheckedIcon
  {
    get => (IconElement)GetValue(CheckedIconProperty);
    set => SetValue(CheckedIconProperty, value);
  }

  public static readonly DependencyProperty CheckedLabelProperty = DependencyProperty.Register("CheckedLabel", typeof(string), typeof(AlternateAppBarToggleButton), new PropertyMetadata(null));
  public string CheckedLabel
  {
    get => (string)GetValue(CheckedLabelProperty);
    set => SetValue(CheckedLabelProperty, value);
  }
}
