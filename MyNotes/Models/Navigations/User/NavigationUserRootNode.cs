using System.Diagnostics.CodeAnalysis;

using MyNotes.Domain.Navigations;
using MyNotes.Strings;

namespace MyNotes.Models.Navigations.User;

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