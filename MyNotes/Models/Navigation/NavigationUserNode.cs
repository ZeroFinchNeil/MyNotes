using CommunityToolkit.Mvvm.ComponentModel;

namespace MyNotes.Models.Navigation;

internal class NavigationUserNode : ObservableObject, INavigationUserNode
{
  public required NavigationId Id { get; init; }

  public required IconSource? Icon
  {
    get;
    set => SetProperty(ref field, value);
  }

  public required string Title
  {
    get;
    set => SetProperty(ref field, value);
  }

  public required Type PageType
  {
    get;
    set => SetProperty(ref field, value);
  }

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

  public NavigationUserCompositeNode? FindParentNode() => this != NavigationUserRootNode.Instance
      ? FindUserNode(node => node is NavigationUserCompositeNode composite && composite.ChildNodes.Contains(this)) as NavigationUserCompositeNode
      : null;

  public NavigationUserNode? FindPreviousNode()
  {
    var parent = FindParentNode();
    if (parent is null)
      return null;

    int index = parent.ChildNodes.IndexOf(this);
    return index > 0 ? parent.ChildNodes[index - 1] : null;
  }

  public NavigationUserNode? FindNextNode()
  {
    var parent = FindParentNode();
    if (parent is null)
      return null;

    int index = parent.ChildNodes.IndexOf(this);
    return index >= 0 && index < parent.ChildNodes.Count - 1 ? parent.ChildNodes[index + 1] : null;
  }

  public bool TryFindRelations(out NavigationUserCompositeNode? parentNode, out NavigationUserNode? previousNode, out NavigationUserNode? nextNode)
  {
    previousNode = null;
    nextNode = null;

    parentNode = FindParentNode();
    if (parentNode is null)
      return false;

    int index = parentNode.ChildNodes.IndexOf(this);
    if (index < 0)
      return false;

    previousNode = index > 0 ? parentNode.ChildNodes[index - 1] : null;
    nextNode = index < parentNode.ChildNodes.Count - 1 ? parentNode.ChildNodes[index + 1] : null;

    return true;
  }
}

#region User Nodes

internal class NavigationUserCompositeNode : NavigationUserNode
{
  public NavigationUserNodeCollection ChildNodes { get; } = new();
}

internal class NavigationUserLeafNode : NavigationUserNode
{ }

internal sealed class NavigationUserRootNode : NavigationUserCompositeNode
{
  public static NavigationUserRootNode Instance => field ??= new()
  {
    Id = NavigationId.UserRootNode,
    Icon = null,
    Title = "",
    PageType = typeof(Page),
    Position = 0
  };

  private NavigationUserRootNode() { }
}
#endregion
