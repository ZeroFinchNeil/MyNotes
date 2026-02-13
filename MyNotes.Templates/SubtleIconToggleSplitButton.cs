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

  public static readonly DependencyProperty IconViewBoxWidthProperty = DependencyProperty.Register("IconViewBoxWidth", typeof(double), typeof(SubtleIconToggleSplitButton), new PropertyMetadata(16.0));
  public double IconViewBoxWidth
  {
    get => (double)GetValue(IconViewBoxWidthProperty);
    set => SetValue(IconViewBoxWidthProperty, value);
  }

  public static readonly DependencyProperty IconViewBoxHeightProperty = DependencyProperty.Register("IconViewBoxHeight", typeof(double), typeof(SubtleIconToggleSplitButton), new PropertyMetadata(16.0));
  public double IconViewBoxHeight
  {
    get => (double)GetValue(IconViewBoxHeightProperty);
    set => SetValue(IconViewBoxHeightProperty, value);
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
