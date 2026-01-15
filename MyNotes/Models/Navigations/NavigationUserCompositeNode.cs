using MyNotes.Resources;
using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations;

internal class NavigationUserCompositeNode : NavigationUserNode
{
  public NavigationUserNodeCollection ChildNodes { get; }

  public NavigationUserCompositeNode() : base(typeof(HomePage)) { ChildNodes = new(this); }

  public bool IsExpanded
  {
    get;
    set => SetProperty(ref field, value);
  }

  public void ForEachDescendant(Action<NavigationUserNode> action)
  {
    Stack<NavigationUserNode> stack = new();
    stack.Push(this);

    while (stack.Count > 0)
    {
      var node = stack.Pop();
      action.Invoke(node);

      if (node is NavigationUserCompositeNode compositeNode)
      {
        foreach (var childNode in compositeNode.ChildNodes)
          stack.Push(childNode);
      }
    }
  }
}

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
