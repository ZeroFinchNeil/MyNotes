using MyNotes.Debugging;

namespace MyNotes.Models.Navigations;

internal sealed class NavigationSeparator : INavigation
{
  public NavigationSeparator()
  {
#if DEBUG
    ReferenceTracker.NavigationReference.Add(this, $"{GetType().Name.Replace("Navigation", ""),15}: {GetHashCode()}");
#endif
  }
}
