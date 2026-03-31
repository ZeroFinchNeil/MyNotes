using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace MyNotes.Debugging;

internal static class FocusTracker
{
  public static FrameworkElement? GetFocusedElement(XamlRoot xamlRoot) => FocusManager.GetFocusedElement(xamlRoot) is FrameworkElement focusedElement ? focusedElement : null;
}
