using MyNotes.Resources;

namespace MyNotes.Models.Navigations;

[Debugging.ReferenceTracker]
internal sealed class NavigationUserRootNode : NavigationUserCompositeNode
{
  public static NavigationUserRootNode Instance => field ??= new()
  {
    Id = NavigationId.UserRootNode,
    Parent = null!,
    Icon = Templates.Icon.System_Library,
    Title = LocalizedStrings.NavigationUserRootNodeDisplayName,
    PageType = typeof(Page),
    Position = 0
  };

  private NavigationUserRootNode()
  {
    Parent = this;
  }
}