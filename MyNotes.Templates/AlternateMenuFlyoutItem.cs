using CommunityToolkit.WinUI.Helpers;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyNotes.Templates;

public sealed partial class AlternateMenuFlyoutItem : MenuFlyoutItem
{
  public AlternateMenuFlyoutItem()
  {
    DefaultStyleKey = typeof(AlternateMenuFlyoutItem);
    ClickWeakEventListener = new(this)
    {
      OnEventAction = (instance, source, args) => instance.IsChecked = !instance.IsChecked,
      OnDetachAction = (weakEventListener) => this.Click -= weakEventListener.OnEvent
    };

    this.Loaded += AlternateMenuFlyoutItem_Loaded;
    //this.Unloaded += AlternateMenuFlyoutItem_Unloaded;
  }

  public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(AlternateMenuFlyoutItem), new PropertyMetadata(false, OnIsCheckedChanged));
  public bool IsChecked
  {
    get => (bool)GetValue(IsCheckedProperty);
    set => SetValue(IsCheckedProperty, value);
  }

  private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is AlternateMenuFlyoutItem control && control.IsLoaded)
    {
      if (e.NewValue is true)
      {
        VisualStateManager.GoToState(control, "Checked", false);
      }
      else
      {
        VisualStateManager.GoToState(control, "Unchecked", false);
      }
    }
  }

  public static readonly DependencyProperty CheckedIconProperty = DependencyProperty.Register("CheckedIcon", typeof(IconElement), typeof(AlternateMenuFlyoutItem), new PropertyMetadata(null));
  public IconElement CheckedIcon
  {
    get => (IconElement)GetValue(CheckedIconProperty);
    set => SetValue(CheckedIconProperty, value);
  }

  public static readonly DependencyProperty CheckedTextProperty = DependencyProperty.Register("CheckedText", typeof(string), typeof(AlternateMenuFlyoutItem), new PropertyMetadata(null));
  public string CheckedText
  {
    get => (string)GetValue(CheckedTextProperty);
    set => SetValue(CheckedTextProperty, value);
  }

  private readonly WeakEventListener<AlternateMenuFlyoutItem, object, RoutedEventArgs> ClickWeakEventListener;

  protected override void OnApplyTemplate()
  {
    InitializeVisualState();
  }

  private void AlternateMenuFlyoutItem_Loaded(object sender, RoutedEventArgs e)
  {
    InitializeVisualState();
    DetachWeakEventHandler();
    AttachWeakEventHandler();
  }

  private void InitializeVisualState()
  {
    if (IsChecked)
    {
      VisualStateManager.GoToState(this, "Checked", false);
    }
    else
    {
      VisualStateManager.GoToState(this, "Unchecked", false);
    }
  }

  private void AlternateMenuFlyoutItem_Unloaded(object sender, RoutedEventArgs e)
  {
    DetachWeakEventHandler();
  }

  private void AttachWeakEventHandler()
  {
    this.Click += ClickWeakEventListener.OnEvent;
  }

  private void DetachWeakEventHandler()
  {
    this.Click -= ClickWeakEventListener.OnEvent;
  }
}
