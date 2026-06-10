using MyNotes.Shared.Constants;
using MyNotes.Domain.ValueObjects;

using System.Diagnostics.CodeAnalysis;

namespace MyNotes.Models.Navigations;

[Debugging.ReferenceTracker]
internal sealed partial class NavigationUserRootNode : NavigationUserCompositeNode
{
  [SetsRequiredMembers]
  public NavigationUserRootNode()
  {
    Id = NavigationId.UserRoot;
    Icon = Templates.Icon.System_Library;
    Title = LocalizedStrings.NavigationUserRootNodeDisplayName;
    PageType = typeof(Page);
    Parent = this;
  }
}