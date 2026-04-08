using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Templates;

namespace MyNotes.Models.Navigations;

[Debugging.ReferenceTracker]
internal abstract partial class NavigationUserNode : ObservableObject, INavigationNode
{
  public NavigationUserNode(Type pageType)
  {
    TrackReference();
    PageType = pageType;
  }

  public required NavigationId Id { get; init; }
  public Type PageType { get; init; }

  [ObservableProperty]
  public required partial NavigationUserCompositeNode Parent { get; set; }

  [ObservableProperty]
  public required partial Icon Icon { get; set; }

  [ObservableProperty]
  public required partial string Title { get; set; }

  [ObservableProperty]
  public required partial int Position { get; set; }

  public static NavigationUserNode? FindUserNode(Func<NavigationUserNode, bool> func)
  {
    Stack<NavigationUserNode> stack = new();
    stack.Push(NavigationUserRootNode.Instance);

    while (stack.Count > 0)
    {
      var node = stack.Pop();
      if (func.Invoke(node))
      {
        return node;
      }

      if (node is NavigationUserCompositeNode compositeNode)
      {
        foreach (var childNode in compositeNode.ChildNodes)
        {
          stack.Push(childNode);
        }
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

  public override string ToString() => $"{Id.Value} ({Title})";
}
