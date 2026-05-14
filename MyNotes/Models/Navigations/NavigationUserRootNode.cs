using MyNotes.Shared.Constants;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Models.Navigations;

[Debugging.ReferenceTracker]
internal sealed partial class NavigationUserRootNode : NavigationUserCompositeNode
{
  public static NavigationUserRootNode Instance => field ??= new()
  {
    Id = NavigationId.UserRoot,
    Parent = null!,
    Icon = Templates.Icon.System_Library,
    Title = LocalizedStrings.NavigationUserRootNodeDisplayName,
    PageType = typeof(Page)
  };

  private NavigationUserRootNode()
  {
    Parent = this;
  }
}