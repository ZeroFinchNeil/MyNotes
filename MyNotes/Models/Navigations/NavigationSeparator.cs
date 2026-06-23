namespace MyNotes.Models.Navigations;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class NavigationSeparator : INavigation
{
  public NavigationSeparator()
  {
    TrackReference();
  }
}
