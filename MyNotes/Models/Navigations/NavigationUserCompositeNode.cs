using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations;

internal partial class NavigationUserCompositeNode : NavigationUserNode
{
#if false
  public NavigationUserNodeCollection ChildNodes { get; }

  public NavigationUserCompositeNode() : base(typeof(HomePage)) { ChildNodes = new(this); }
#endif

  public ObservableCollection<NavigationUserNode> ChildNodes { get; } = new();

  public NavigationUserCompositeNode() : base(typeof(HomePage)) { }

  [ObservableProperty]
  public partial bool IsExpanded { get; set; }

  public void ForEachDescendant(Action<NavigationUserNode> action, bool containsSelf = true)
  {
    Queue<NavigationUserNode> queue = new();
    queue.Enqueue(this);

    while (queue.Count > 0)
    {
      var node = queue.Dequeue();
      if (containsSelf || this != node)
      {
        action.Invoke(node);
      }

      if (node is NavigationUserCompositeNode compositeNode)
      {
        foreach (var childNode in compositeNode.ChildNodes)
        {
          queue.Enqueue(childNode);
        }
      }
    }
  }

  public IReadOnlyList<NavigationUserNode> FindDescendants(Func<NavigationUserNode, bool> condition, bool containsSelf = true)
  {
    List<NavigationUserNode> resultNodes = new();

    Stack<NavigationUserNode> stack = new();
    stack.Push(this);

    while (stack.Count > 0)
    {
      var node = stack.Pop();
      if (containsSelf || this != node)
      {
        if (condition.Invoke(node))
        {
          resultNodes.Add(node);
        }
      }

      if (node is NavigationUserCompositeNode compositeNode)
      {
        int index = compositeNode.ChildNodes.Count - 1;
        for (int i = index; i >= 0; i--)
        {
          stack.Push(compositeNode.ChildNodes[i]);
        }
      }
    }

    return resultNodes;
  }

  public bool AnyDescendant(Func<NavigationUserNode, bool> condition, bool containsSelf = true)
  {
    Queue<NavigationUserNode> queue = new();
    queue.Enqueue(this);

    while (queue.Count > 0)
    {
      var node = queue.Dequeue();
      if (containsSelf || this != node)
      {
        if (condition.Invoke(node))
        {
          return true;
        }
      }

      if (node is NavigationUserCompositeNode compositeNode)
      {
        foreach (var childNode in compositeNode.ChildNodes)
        {
          queue.Enqueue(childNode);
        }
      }
    }

    return false;
  }

  public bool IsParentOf(NavigationUserNode node) => node.Parent == this && ChildNodes.Contains(node);
  public bool HasDescendant(NavigationUserNode node) => AnyDescendant(n => n == node, false);

  public bool CanBeParentOf(NavigationUserNode node) => this != node && node switch
  {
    NavigationUserLeafNode leaf => !IsParentOf(leaf),
    NavigationUserCompositeNode composite => !IsParentOf(composite) && !composite.HasDescendant(this),
    _ => false
  };
}