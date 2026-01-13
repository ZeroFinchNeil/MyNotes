using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace MyNotes.Templates;

public sealed partial class SubtleIconToggleSplitButton : ToggleButton
{
  public SubtleIconToggleSplitButton()
  {
    DefaultStyleKey = typeof(SubtleIconToggleSplitButton);
  }

  public static readonly DependencyProperty FlyoutProperty = DependencyProperty.Register("Flyout", typeof(FlyoutBase), typeof(SubtleIconToggleSplitButton), new PropertyMetadata(null));
  public FlyoutBase Flyout
  {
    get => (FlyoutBase)GetValue(FlyoutProperty);
    set => SetValue(FlyoutProperty, value);
  }

  protected override void OnApplyTemplate()
  {
    base.OnApplyTemplate();
    Flyout?.Opened += Flyout_Opened;
    Flyout?.Closed += Flyout_Closed;
  }

  private void Flyout_Closed(object? sender, object e)
  {
    this.IsEnabled = !Flyout.IsOpen;
  }

  private void Flyout_Opened(object? sender, object e)
  {
    this.IsEnabled = !Flyout.IsOpen;
  }
}
