using MyNotes.Common.Commands;
using MyNotes.Models.Navigation;
using MyNotes.Services.Database.Entities;
using MyNotes.Views.Navigations;

namespace MyNotes.ViewModels;

internal sealed partial class MainViewModel : ViewModelBase
{
  public Command<INavigationNode>? AddListCommand { get; private set; }
  public Command<INavigationNode>? AddGroupCommand { get; private set; }
  public Command? SetMovableNavigationsCommand { get; private set; }

  private async Task AddUserNode(INavigationNode navigation, bool isCompositeNode)
  {
    NavigationUserNode? node = navigation switch
    {
      NavigationUserLeafNode leaf => leaf,
      NavigationUserCompositeNode composite => composite.ChildNodes.LastOrDefault(),
      _ => UserRootNavigation.ChildNodes.LastOrDefault()
    };

    NavigationUserNode newNode = isCompositeNode
      ? new NavigationUserCompositeNode()
      {
        Id = NavigationId.NewId(),
        Icon = new SymbolIconSource() { Symbol = Symbol.Bookmarks },
        Title = "Composite " + new Random().Next(10000),
        PageType = typeof(HomePage),
        Position = int.MaxValue
      }
      : new NavigationUserLeafNode()
      {
        Id = NavigationId.NewId(),
        Icon = new SymbolIconSource() { Symbol = Symbol.Bookmarks },
        Title = "Leaf " + new Random().Next(10000),
        PageType = typeof(HomePage),
        Position = int.MaxValue
      };

    NavigationUserCompositeNode? parentNode = node?.FindParentNode();

    if (node is not null && parentNode is not null)
    {
      int index = parentNode.ChildNodes.IndexOf(node);
      parentNode.ChildNodes.Insert(index + 1, newNode);
    }
    else
    {
      parentNode = navigation switch
      {
        NavigationUserCompositeNode composite => composite,
        NavigationUserLeafNode leaf => leaf.FindParentNode() ?? UserRootNavigation,
        _ => UserRootNavigation
      };

      newNode.Position = parentNode.ChildNodes.Count > 0 ? parentNode.ChildNodes[^1].Position + 1 : 0;
      parentNode.ChildNodes.Add(newNode);
    }

    NavigationEntity entity = new()
    {
      Id = newNode.Id.Value,
      Title = newNode.Title,
      Parent = parentNode.Id.Value,
      Position = newNode.Position,
      IsComposite = isCompositeNode
    };

    await using (var context = await _dbContextFactory.CreateDbContextAsync())
    {
      await context.NavigationEntities.AddAsync(entity);
      await context.SaveChangesAsync();
    }

    newNode.PropertyChanged += UserNode_PropertyChanged;
  }

  private void SetCommands()
  {
    AddListCommand = new(
      actionToExecute: async (navigation) => await AddUserNode(navigation: navigation, isCompositeNode: false),
      canExecuteFunc: navigation => navigation is INavigationUserNode
      );

    AddGroupCommand = new(
      actionToExecute: async (navigation) => await AddUserNode(navigation: navigation, isCompositeNode: true),
      canExecuteFunc: navigation => navigation is INavigationUserNode
      );

    SetMovableNavigationsCommand = new(
      actionToExecute: async () =>
      {

      });
  }
}