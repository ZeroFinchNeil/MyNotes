using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations;

internal partial class NavigationUserCompositeNode : NavigationUserNode
{
  public ObservableCollection<NavigationUserNode> ChildNodes { get; } = new();

  public NavigationUserCompositeNode() : base(typeof(HomePage)) { }

  [ObservableProperty]
  public partial bool IsExpanded { get; set; }

  public void ForEachDescendant(Action<NavigationUserNode> action, bool containsSelf = true)
  {
    if (containsSelf)
    {
      action.Invoke(this);
    }

    Queue<NavigationUserNode> queue = new();
    queue.Enqueue(this);

    while (queue.Count > 0)
    {
      var currentNode = queue.Dequeue();
      if (!ReferenceEquals(currentNode, this))
      {
        action.Invoke(currentNode);
      }

      if (currentNode is NavigationUserCompositeNode compositeNode)
      {
        foreach (var childNode in compositeNode.ChildNodes)
        {
          queue.Enqueue(childNode);
        }
      }
    }
  }

  public bool AnyDescendant(Func<NavigationUserNode, bool> predicate, bool containsSelf = true)
  {
    if (containsSelf && predicate.Invoke(this))
    {
      return true;
    }

    Queue<NavigationUserNode> queue = new();
    queue.Enqueue(this);

    while (queue.Count > 0)
    {
      var currentNode = queue.Dequeue();
      if (predicate.Invoke(currentNode))
      {
        return true;
      }

      if (currentNode is NavigationUserCompositeNode compositeNode)
      {
        foreach (var childNode in compositeNode.ChildNodes)
        {
          queue.Enqueue(childNode);
        }
      }
    }

    return false;
  }

  public bool TryGetFirstDescendant(Func<NavigationUserNode, bool> predicate, [NotNullWhen(true)] out NavigationUserNode? node, bool containsSelf = true)
  {
    if (containsSelf && predicate.Invoke(this))
    {
      node = this;
      return true;
    }

    Stack<NavigationUserNode> stack = new();
    PushChildren(stack, this);

    while (stack.Count > 0)
    {
      var currentNode = stack.Pop();

      if (predicate.Invoke(currentNode))
      {
        node = currentNode;
        return true;
      }

      if (currentNode is NavigationUserCompositeNode currentCompositeNode)
      {
        PushChildren(stack, currentCompositeNode);
      }
    }

    node = null;
    return false;
  }

  public IReadOnlyList<NavigationUserNode> FindDescendants(Func<NavigationUserNode, bool> predicate, bool containsSelf = true)
  {
    List<NavigationUserNode> resultNodes = new();

    if (containsSelf && predicate.Invoke(this))
    {
      resultNodes.Add(this);
    }

    Stack<NavigationUserNode> stack = new();
    stack.Push(this);

    while (stack.Count > 0)
    {
      var currentNode = stack.Pop();
      if (predicate.Invoke(currentNode) && !ReferenceEquals(currentNode, this))
      {
        resultNodes.Add(currentNode);
      }

      if (currentNode is NavigationUserCompositeNode compositeNode)
      {
        PushChildren(stack, compositeNode);
      }
    }

    return resultNodes;
  }

  private static void PushChildren(Stack<NavigationUserNode> stack, NavigationUserCompositeNode parentNode)
  {
    for (int i = parentNode.ChildNodes.Count - 1; i >= 0; i--)
    {
      stack.Push(parentNode.ChildNodes[i]);
    }
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