using Microsoft.EntityFrameworkCore;

using MyNotes.Common.Commands;
using MyNotes.Models;
using MyNotes.Services.Database;
using MyNotes.Services.Database.Entities;
using MyNotes.Views.Navigations;

namespace MyNotes.ViewModels;

internal sealed partial class MainViewModel : ViewModelBase
{
  private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

  private readonly ImmutableList<INavigation> PrimaryCoreNavigations;
  public NavigationUserRootNode UserRootNavigation { get; } = NavigationUserRootNode.Instance;
  private readonly ImmutableList<INavigation> SecondaryCoreNavigations;

  public CollectionViewSource MenuItems { get; } = new() { IsSourceGrouped = true };
  public IReadOnlyList<INavigation> FooterMenuItems => SecondaryCoreNavigations;
  public IReadOnlyList<INavigationUserNode> UserNavigations => UserRootNavigation.ChildNodes;

  public MainViewModel(IDbContextFactory<AppDbContext> dbContextFactory)
  {
    _dbContextFactory = dbContextFactory;
    SetCommands();

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
}

internal sealed partial class MainViewModel : ViewModelBase
{
  public Command<INavigationNode>? AddListCommand { get; private set; }

  private void SetCommands()
  {
    AddListCommand = new(
      actionToExecute: async (navigation) =>
      {
        var newNode = new NavigationUserLeafNode()
        {
          Id = NavigationId.Create(),
          Icon = new SymbolIconSource() { Symbol = Symbol.Bookmarks },
          Title = "Leaf",
          PageType = typeof(HomePage)
        };
        NavigationUserNode? node = navigation switch
        {
          NavigationUserLeafNode leaf => leaf,
          NavigationUserCompositeNode composite => composite.ChildNodes.LastOrDefault(),
          _ => UserRootNavigation.ChildNodes.LastOrDefault()
        };

        if (node is not null && node.TryFindRelations(out var parentNode, out var previousNode, out var nextNode) && parentNode is not null)
        {
          parentNode.ChildNodes.Add(newNode);
          NavigationEntity entity = new()
          {
            Id = newNode.Id.Value,
            Title = newNode.Title,
            Parent = parentNode.Id.Value,
            Next = nextNode is null ? Guid.Empty : nextNode.Id.Value
          };
          await using (var context = await _dbContextFactory.CreateDbContextAsync())
          {
            if (previousNode is not null)
            {
              var previous = await context.NavigationEntities.FirstOrDefaultAsync(e => e.Id == previousNode.Id.Value);
              previous?.Next = entity.Id;
            }
            await context.NavigationEntities.AddAsync(entity);
            await context.SaveChangesAsync();
          }
        }
        else
        {
          NavigationUserCompositeNode parent = navigation switch
          {
            NavigationUserCompositeNode composite => composite,
            NavigationUserLeafNode leaf => leaf.FindParentNode() ?? UserRootNavigation,
            _ => UserRootNavigation
          };

          parent.ChildNodes.Add(newNode);
          NavigationEntity entity = new()
          {
            Id = newNode.Id.Value,
            Title = newNode.Title,
            Parent = parent.Id.Value,
            Next = Guid.Empty
          };
          await using (var context = await _dbContextFactory.CreateDbContextAsync())
          {
            await context.NavigationEntities.AddAsync(entity);
            await context.SaveChangesAsync();
          }
        }
      },
      canExecuteFunc: navigation => navigation is INavigationUserNode
      );
  }
}