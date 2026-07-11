using System.Diagnostics.CodeAnalysis;

using MyNotes.Domain.ValueObjects;
using MyNotes.Shared.Constants;

namespace MyNotes.Models.Navigations;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class NavigationUserRootNode : NavigationUserCompositeNode
{
  public static NavigationUserRootNode Instance { get; } = new();

  [SetsRequiredMembers]
  private NavigationUserRootNode()
  {
    Id = NavigationId.UserRoot;
    Icon = Templates.Icon.System_Library;
    Title = LocalizedStrings.NavigationUserRootNodeDisplayName;
    PageType = typeof(Page);
    Parent = this;
  }
}