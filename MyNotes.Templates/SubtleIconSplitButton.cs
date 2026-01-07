using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace MyNotes.Templates;

public sealed partial class SubtleIconSplitButton : Button
{
  public SubtleIconSplitButton()
  {
    DefaultStyleKey = typeof(SubtleIconSplitButton);
  }

  public static readonly DependencyProperty SecondaryFlyoutProperty = DependencyProperty.Register("SecondaryFlyout", typeof(FlyoutBase), typeof(SubtleIconSplitButton), new PropertyMetadata(null));
  public FlyoutBase SecondaryFlyout
  {
    get => (FlyoutBase)GetValue(SecondaryFlyoutProperty);
    set => SetValue(SecondaryFlyoutProperty, value);
  }

  protected override void OnApplyTemplate()
  {
    SecondaryFlyout?.Opened += SubtleIconSplitButton_Opened;
    SecondaryFlyout?.Closed += SubtleIconSplitButton_Closed;
  }

  private void SubtleIconSplitButton_Closed(object? sender, object e)
  {
    this.IsEnabled = !SecondaryFlyout.IsOpen;
  }

  private void SubtleIconSplitButton_Opened(object? sender, object e)
  {
    this.IsEnabled = !SecondaryFlyout.IsOpen;
  }
}
