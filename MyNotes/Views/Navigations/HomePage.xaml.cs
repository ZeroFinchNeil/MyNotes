using MyNotes.Debugging;

namespace MyNotes.Views.Navigations;

internal sealed partial class HomePage : Page
{
  public HomePage()
  {
#if DEBUG
    if (Debugger.IsAttached)
    {
      ReferenceTracker.PageReference.Add(this, $"{GetType().Name}: {GetHashCode()}");
    }
#endif

    InitializeComponent();
  }
}
