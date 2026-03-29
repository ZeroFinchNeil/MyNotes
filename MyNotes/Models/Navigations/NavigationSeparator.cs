namespace MyNotes.Models.Navigations;

[Debugging.ReferenceTracker]
internal sealed partial class NavigationSeparator : INavigation
{
  public NavigationSeparator()
  {
    TrackReference();
  }
}
