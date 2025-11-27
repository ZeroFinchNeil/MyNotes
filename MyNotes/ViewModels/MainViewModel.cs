using MyNotes.Models;
using MyNotes.Views.Navigations;

namespace MyNotes.ViewModels;

public partial class MainViewModel : ViewModelBase
{
  private readonly ImmutableList<INavigation> PrimaryCoreNavigations;
  private readonly NavigationUserRootNode UserRootNavigation = NavigationUserRootNode.Instance;
  private readonly ImmutableList<INavigation> SecondaryCoreNavigations;

  public CollectionViewSource MenuItems { get; } = new() { IsSourceGrouped = true };
  public IReadOnlyList<INavigation> FooterMenuItems => SecondaryCoreNavigations;
  public IReadOnlyList<INavigationUserNode> UserNavigations => UserRootNavigation.ChildNodes;

  public MainViewModel()
  {
    PrimaryCoreNavigations = [NavigationHome.Instance, NavigationBookmarks.Instance, new NavigationSeparator()];
    SecondaryCoreNavigations = [new NavigationSeparator(), NavigationTrash.Instance, NavigationSettings.Instance];

    var composite1 = new NavigationUserCompositeNode()
    {
      Id = NavigationId.Create(),
      Icon = new SymbolIconSource() { Symbol = Symbol.Bookmarks },
      Title = "Composite 1",
      PageType = typeof(HomePage)
    };
    var composite2 = new NavigationUserCompositeNode()
    {
      Id = NavigationId.Create(),
      Icon = new SymbolIconSource() { Symbol = Symbol.Bookmarks },
      Title = "Composite 2",
      PageType = typeof(HomePage)
    };
    var leaf1 = new NavigationUserLeafNode()
    {
      Id = NavigationId.Create(),
      Icon = new SymbolIconSource() { Symbol = Symbol.Bookmarks },
      Title = "Leaf 1",
      PageType = typeof(HomePage)
    };
    var leaf2 = new NavigationUserLeafNode()
    {
      Id = NavigationId.Create(),
      Icon = new SymbolIconSource() { Symbol = Symbol.Bookmarks },
      Title = "Leaf 2",
      PageType = typeof(HomePage)
    };
    composite1.ChildNodes.Add(leaf1);
    composite1.ChildNodes.Add(composite2);
    UserRootNavigation.ChildNodes.Add(composite1);
    UserRootNavigation.ChildNodes.Add(leaf2);

    IReadOnlyList<IReadOnlyList<INavigation>> MenuItemsSource = [PrimaryCoreNavigations, UserNavigations];
    MenuItems.Source = MenuItemsSource;
  }

  public NavigationUserNode? GetUserNode(Func<NavigationUserNode, bool> func)
  {
    Stack<NavigationUserNode> stack = new();
    stack.Push(UserRootNavigation);

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
}