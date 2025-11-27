using System;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Controls;

namespace MyNotes.Models;

public interface INavigation { }

public sealed class NavigationSeparator : INavigation { }

public interface INavigationNode : INavigation
{
  public NavigationId Id { get; }
  public string Title { get; set; }
  public Type PageType { get; set; }
}

#region Core Nodes
public interface INavigationCoreNode : INavigationNode { }

public abstract class NavigationCoreNode : ObservableObject, INavigationCoreNode
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
#endregion

#region User Nodes
public interface INavigationUserNode : INavigationNode { }

public class NavigationUserNode : ObservableObject, INavigationUserNode
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
}

public class NavigationUserCompositeNode : NavigationUserNode
{
  public ObservableCollection<NavigationUserNode> ChildNodes { get; } = new();
}

public class NavigationUserLeafNode : NavigationUserNode
{
}

public sealed class NavigationUserRootNode : NavigationUserCompositeNode
{
  public static NavigationUserRootNode Instance => field ??= new()
  {
    Id = NavigationId.Empty,
    Icon = null,
    Title = "",
    PageType = typeof(Page),
  };

  private NavigationUserRootNode() { }
}
#endregion

public readonly struct NavigationId : IEquatable<NavigationId>
{
  public static NavigationId Empty { get; } = new(Guid.Empty);
  public static NavigationId Home { get; } = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));
  public static NavigationId Bookmarks { get; } = new(Guid.Parse("00000000-0000-0000-0000-000000000002"));
  public static NavigationId Tags { get; } = new(Guid.Parse("00000000-0000-0000-0000-000000000003"));

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