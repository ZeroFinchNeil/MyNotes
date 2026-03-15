using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

using Windows.UI;

namespace MyNotes.Templates.Media;

public abstract partial class ExtendedSystemBackdrop : SystemBackdrop
{
  public static readonly DependencyProperty TintColorProperty = DependencyProperty.Register("TintColor", typeof(Color), typeof(ExtendedSystemBackdrop), new PropertyMetadata(Color.FromArgb(255, 252, 252, 252), OnBackdropPropertyChangedCore));
  public Color TintColor
  {
    get => (Color)GetValue(TintColorProperty);
    set => SetValue(TintColorProperty, value);
  }

  public static readonly DependencyProperty TintOpacityProperty = DependencyProperty.Register("TintOpacity", typeof(double), typeof(ExtendedSystemBackdrop), new PropertyMetadata(0.0, OnBackdropPropertyChangedCore));
  public double TintOpacity
  {
    get => (double)GetValue(TintOpacityProperty);
    set => SetValue(TintOpacityProperty, value);
  }

  public static readonly DependencyProperty LuminosityOpacityProperty = DependencyProperty.Register("LuminosityOpacity", typeof(double), typeof(ExtendedSystemBackdrop), new PropertyMetadata(0.85, OnBackdropPropertyChangedCore));
  public double LuminosityOpacity
  {
    get => (double)GetValue(LuminosityOpacityProperty);
    set => SetValue(LuminosityOpacityProperty, value);
  }

  public static readonly DependencyProperty FallbackColorProperty = DependencyProperty.Register("FallbackColor", typeof(Color), typeof(ExtendedSystemBackdrop), new PropertyMetadata(Color.FromArgb(255, 249, 249, 249), OnBackdropPropertyChangedCore));
  public Color FallbackColor
  {
    get => (Color)GetValue(FallbackColorProperty);
    set => SetValue(FallbackColorProperty, value);
  }

  private static void OnBackdropPropertyChangedCore(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is ExtendedSystemBackdrop backdrop)
    {
      backdrop.OnBackdropPropertyChanged(backdrop, e);
    }
  }

  protected abstract void OnBackdropPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e);
}
