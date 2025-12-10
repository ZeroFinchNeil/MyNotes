using CommunityToolkit.Mvvm.Messaging;

using Microsoft.UI.Xaml.Media.Imaging;

using MyNotes.Common.Commands;
using MyNotes.Common.Messages;
using MyNotes.Models.Navigation;
using MyNotes.Resources;
using MyNotes.Services.Database.Entities;
using MyNotes.Views.Navigations;

namespace MyNotes.ViewModels;

internal sealed partial class MainViewModel : DisposableViewModelBase
{
  public Command<INavigationNode>? AddListCommand { get; private set; }
  public Command<INavigationNode>? AddGroupCommand { get; private set; }
  public Command? SetMovableNavigationsCommand { get; private set; }
  public Command<NavigationUserNode>? ExitUserNodeEditCommand { get; private set; }

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
        Icon = "1f600",
        Title = "Composite " + new Random().Next(10000),
        PageType = typeof(HomePage),
        Position = int.MaxValue
      }
      : new NavigationUserLeafNode()
      {
        Id = NavigationId.NewId(),
        Icon = "1f600",
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
      Icon = newNode.Icon,
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

    CurrentNavigation = newNode;
    newNode.IsEditable = true;

    //var message = new ExtendedRequestMessage<NavigationUserNode, bool>() { Request = newNode };
    //WeakReferenceMessenger.Default.Send(message, MessageTokens.ChangeUserNodeFocustState);

    //Console.WriteLine(message.Response);
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

    ExitUserNodeEditCommand = new(node => node.IsEditable = false);
  }
}