namespace MyNotes.Debugging;

internal static class FocusTracker
{
  public static void GetFocusedElement(XamlRoot xamlRoot)
  {
    if (FocusManager.GetFocusedElement(xamlRoot) is FrameworkElement focusedElement)
    {
      Console.WriteLine("{0}: {1}", "Focused Element", focusedElement.GetType());
    }
  }
}
