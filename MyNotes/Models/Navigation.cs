using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Controls.AnimatedVisuals;

using MyNotes.Resources;
using MyNotes.Views.Navigations;

namespace MyNotes.Models;

internal interface INavigation { }

internal sealed class NavigationSeparator : INavigation { }

internal interface INavigationNode : INavigation
{
  public NavigationId Id { get; }
  public string Title { get; set; }
  public Type PageType { get; set; }
}

#region Core Nodes
internal interface INavigationCoreNode : INavigationNode { }

internal abstract class NavigationCoreNode : ObservableObject, INavigationCoreNode
{
  public required NavigationId Id { get; init; }

  public required IconElement Icon
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
}

internal sealed class NavigationHome : NavigationCoreNode
{
  public static NavigationHome Instance => field ??= new()
  {
    Id = NavigationId.Home,
    Icon = new IconSourceElement() { IconSource = new SymbolIconSource() { Symbol = Symbol.Home } },
    Title = LocalizedStrings.NavigationHomeTitle,
    PageType = typeof(HomePage)
  };

  private NavigationHome() { }
}

internal sealed class NavigationBookmarks : NavigationCoreNode
{
  public static NavigationBookmarks Instance => field ??= new()
  {
    Id = NavigationId.Bookmarks,
    Icon = new IconSourceElement() { IconSource = new SymbolIconSource() { Symbol = Symbol.Bookmarks } },
    Title = LocalizedStrings.NavigationBookmarksTitle,
    PageType = typeof(HomePage)
  };

  private NavigationBookmarks() { }
}

internal sealed class NavigationTrash : NavigationCoreNode
{
  public static NavigationTrash Instance => field ??= new()
  {
    Id = NavigationId.Empty,
    Icon = new IconSourceElement() { IconSource = new SymbolIconSource() { Symbol = Symbol.Delete } },
    Title = LocalizedStrings.NavigationTrashTitle,
    PageType = typeof(HomePage)
  };

  private NavigationTrash() { }
}

internal sealed class NavigationSettings : NavigationCoreNode
{
  public static NavigationSettings Instance => field ??= new()
  {
    Id = NavigationId.Empty,
    Icon = new AnimatedIcon() { Source = new AnimatedSettingsVisualSource() },
    Title = LocalizedStrings.NavigationSettingsTitle,
    PageType = typeof(SettingsPage)
  };

  private NavigationSettings() { }
}
#endregion

#region User Nodes
internal interface INavigationUserNode : INavigationNode { }

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

internal class NavigationUserCompositeNode : NavigationUserNode
{
  public ObservableCollection<NavigationUserNode> ChildNodes { get; } = new();
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
  };

  private NavigationUserRootNode() { }
}
#endregion

internal readonly struct NavigationId : IEquatable<NavigationId>
{
  public static NavigationId Empty { get; } = new(Guid.Empty);
  public static NavigationId UserRootNode { get; } = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));
  public static NavigationId Home { get; } = new(Guid.Parse("00000000-0000-0000-0000-000000000008"));
  public static NavigationId Bookmarks { get; } = new(Guid.Parse("00000000-0000-0000-0000-000000000009"));
  public static NavigationId Tags { get; } = new(Guid.Parse("00000000-0000-0000-0000-00000000000a"));

  private static readonly Guid _lowerBound = Guid.Parse("00000000-0000-0000-0000-000000000010");
  private static bool IsValidId(Guid id) => id >= _lowerBound;

  public static NavigationId Create()
  {
    Guid id;
    while (true)
    {
      id = Guid.NewGuid();
      if (IsValidId(id))
        break;
    }
    return new(id);
  }
  private NavigationId(Guid id) => Value = id;

  public static NavigationId Create(Guid id) => IsValidId(id) ? new(id) : throw new ArgumentException("");
  public static NavigationId Create(string id) => Create(Guid.Parse(id));

  public Guid Value { get; }

  public static bool operator ==(NavigationId id1, NavigationId id2) => id1.Equals(id2);
  public static bool operator !=(NavigationId id1, NavigationId id2) => !id1.Equals(id2);

  public bool Equals(NavigationId other) => other.Value == Value;
  public override bool Equals(object? obj) => obj is NavigationId navigationId && navigationId.Value == Value;

  public override int GetHashCode() => Value.GetHashCode();

  public override string ToString() => Value.ToString();
}