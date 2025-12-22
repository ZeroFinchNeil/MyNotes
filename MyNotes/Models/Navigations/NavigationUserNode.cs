using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Media.Imaging;

using MyNotes.Helpers;

namespace MyNotes.Models.Navigations;

internal class NavigationUserNode : ObservableObject, INavigationUserNode
{
  public required NavigationId Id { get; init; }

  public required NavigationUserCompositeNode Parent
  {
    get;
    set => SetProperty(ref field, value);
  }

  public required short Icon
  {
    get;
    set
    {
      if (field != value)
      {
        SetProperty(ref field, value);
        _ = ChangeIconImage(value);
      }
    }
  }

  public BitmapImage? IconImage
  {
    get;
    private set => SetProperty(ref field, value);
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

  public bool IsEditable
  {
    get;
    set => SetProperty(ref field, value);
  } = false;

  public override bool Equals(object? obj) => obj is NavigationUserNode node && Id == node.Id;
  public override int GetHashCode() => Id.GetHashCode();

  private async Task ChangeIconImage(short icon) => IconImage = await IconHelper.GetIconImage(icon, (short)Templates.Icon.Emoji_OpenFileFolder, this is NavigationUserCompositeNode);

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

  //public NavigationUserCompositeNode? FindParentNode() => this != NavigationUserRootNode.Instance
  //    ? FindUserNode(node => node is NavigationUserCompositeNode composite && composite.ChildNodes.Contains(this)) as NavigationUserCompositeNode
  //    : null;

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

  //public bool TryFindRelations(out NavigationUserNode? previousNode, out NavigationUserNode? nextNode)
  //{
  //  previousNode = null;
  //  nextNode = null;

  //  int index = Parent.ChildNodes.IndexOf(this);
  //  if (index < 0)
  //    return false;

  //  previousNode = index > 0 ? Parent.ChildNodes[index - 1] : null;
  //  nextNode = index < Parent.ChildNodes.Count - 1 ? Parent.ChildNodes[index + 1] : null;

  //  return true;
  //}
}

#region User Nodes

internal class NavigationUserCompositeNode : NavigationUserNode
{
  public NavigationUserNodeCollection ChildNodes { get; }

  public NavigationUserCompositeNode() { ChildNodes = new(this); }

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
{ }

internal sealed class NavigationUserRootNode : NavigationUserCompositeNode
{
  public static NavigationUserRootNode Instance => field ??= new()
  {
    Id = NavigationId.UserRootNode,
    Parent = null!,
    Icon = (short)Templates.Icon.System_Notebook,
    Title = string.Empty,
    PageType = typeof(Page),
    Position = 0
  };

  private NavigationUserRootNode() 
  {
    Parent = this;
  }
}
#endregion
