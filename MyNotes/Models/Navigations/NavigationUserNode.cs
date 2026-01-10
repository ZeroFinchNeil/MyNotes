using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Resources;
using MyNotes.Templates;
using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations;

internal abstract partial class NavigationUserNode : ObservableObject, INavigationNode
{
  public required NavigationId Id { get; init; }

  public required NavigationUserCompositeNode Parent
  {
    get;
    set => SetProperty(ref field, value);
  }

  public required Icon Icon
  {
    get;
    set => SetProperty(ref field, value);
  }

  public required string Title
  {
    get;
    set => SetProperty(ref field, value);
  }

  public Type PageType { get; init; }

  public NavigationUserNode(Type pageType) { PageType = pageType; }

  public required int Position
  {
    get;
    set => SetProperty(ref field, value);
  }

  public override bool Equals(object? obj) => obj is NavigationUserNode node && Id == node.Id;
  public override int GetHashCode() => Id.GetHashCode();

  public static NavigationUserNode? FindUserNode(Func<NavigationUserNode, bool> func)
  {
    Stack<NavigationUserNode> stack = new();
    stack.Push(NavigationUserRootNode.Instance);

    while (stack.Count > 0)
    {
      var node = stack.Pop();
      if (func.Invoke(node))
        return node;

      if (node is NavigationUserCompositeNode compositeNode)
      {
        foreach (var childNode in compositeNode.ChildNodes)
          stack.Push(childNode);
      }
    }
    return null;
  }

  public NavigationUserNode? FindPreviousNode()
  {
    int index = Parent.ChildNodes.IndexOf(this);
    return index > 0 ? Parent.ChildNodes[index - 1] : null;
  }

  public NavigationUserNode? FindNextNode()
  {
    int index = Parent.ChildNodes.IndexOf(this);
    return index >= 0 && index < Parent.ChildNodes.Count - 1 ? Parent.ChildNodes[index + 1] : null;
  }
}

#region User Nodes

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

internal class NavigationUserLeafNode : NavigationUserNode
{
  public NavigationUserLeafNode() : base(typeof(HomePage)) { PageType = typeof(UserListPage); }
}

internal sealed class NavigationUserRootNode : NavigationUserCompositeNode
{
  public static NavigationUserRootNode Instance => field ??= new()
  {
    Id = NavigationId.UserRootNode,
    Parent = null!,
    Icon = Icon.System_Library,
    Title = LocalizedStrings.NavigationUserRootNodeDisplayName,
    PageType = typeof(Page),
    Position = 0
  };

  private NavigationUserRootNode()
  {
    Parent = this;
  }
}
#endregion
