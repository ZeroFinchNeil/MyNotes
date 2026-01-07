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
}
