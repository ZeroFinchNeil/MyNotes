using System;

using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyNotes.Templates;

public sealed partial class ReadOnlyRichEditBox : RichEditBox
{
  public ReadOnlyRichEditBox()
  {
    DefaultStyleKey = typeof(ReadOnlyRichEditBox);
    IsEnabled = false;

    RegisterIsEnabledPropertyChangedCallback();
    this.Loaded += ReadOnlyRichEditBox_Loaded;
    this.Unloaded += ReadOnlyRichEditBox_Unloaded;
  }

  private void ReadOnlyRichEditBox_Loaded(object sender, RoutedEventArgs e)
  {
    RegisterIsEnabledPropertyChangedCallback();
  }

  private void ReadOnlyRichEditBox_Unloaded(object sender, RoutedEventArgs e)
  {
    UnregisterIsEnabledPropertyChangedCallback();
  }

  private long? _isEnabledPropertyToken = null;
  private void RegisterIsEnabledPropertyChangedCallback()
  {
    if (_isEnabledPropertyToken is not null)
      return;

    _isEnabledPropertyToken = RegisterPropertyChangedCallback(IsEnabledProperty, (d, property) =>
    {
      if ((bool)GetValue(property))
        throw new ArgumentException("ReadOnlyRichEditBox is read-only. IsEnabled property cannot be set to true.", nameof(IsEnabled));

      SetValue(property, false);
    });
  }

  private void UnregisterIsEnabledPropertyChangedCallback()
  {
    if (_isEnabledPropertyToken is long token)
    {
      UnregisterPropertyChangedCallback(IsEnabledProperty, token);
    }
    _isEnabledPropertyToken = null;
  }

  public static readonly DependencyProperty RtfTextProperty = DependencyProperty.Register("RtfText", typeof(string), typeof(ReadOnlyRichEditBox), new PropertyMetadata(null, OnRtfTextChanged));
  public string? RtfText
  {
    get => (string?)GetValue(RtfTextProperty);
    set => SetValue(RtfTextProperty, value);
  }

  private static void OnRtfTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is ReadOnlyRichEditBox control && !control.IsReadOnly)
    {
      control.Document.SetText(TextSetOptions.FormatRtf, e.NewValue as string);
    }
  }

}
