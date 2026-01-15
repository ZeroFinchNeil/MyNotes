using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Templates;

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
