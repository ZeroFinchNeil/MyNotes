using Microsoft.EntityFrameworkCore;

using MyNotes.Common.Commands;
using MyNotes.Models.Navigations;
using MyNotes.Services.Database;
using MyNotes.Services.Database.Entities;
using MyNotes.Services.Dialog;
using MyNotes.Templates;
using MyNotes.ViewModels.Navigations;
using MyNotes.Views.Navigations;

namespace MyNotes.Services.Navigation;

internal sealed partial class NavigationService : IDisposable
{
  private readonly IDbContextFactory<AppDbContext> DbContextFactory;
  private readonly DialogService DialogService;

  public ImmutableList<INavigation> PrimaryCoreNavigations { get; } = [NavigationHome.Instance, NavigationBookmarks.Instance, new NavigationSeparator()];
  public NavigationUserRootNode UserRootNavigation { get; } = NavigationUserRootNode.Instance;
  public ImmutableList<INavigation> SecondaryCoreNavigations { get; } = [new NavigationSeparator(), NavigationTrash.Instance, NavigationSettings.Instance];

  public NavigationService(IDbContextFactory<AppDbContext> dbContextFactory, DialogService dialogService)
  {
    DbContextFactory = dbContextFactory;
    DialogService = dialogService;

    BuildNavigationTask = BuildNavigationTree();
    SetCommands();
  }

  public Task BuildNavigationTask { get; }
  private async Task BuildNavigationTree()
  {
    await using var context = await DbContextFactory.CreateDbContextAsync();
    var entities = context.NavigationEntities.AsEnumerable();
    var nodes = entities
      .Select<NavigationEntity, NavigationUserNode>(e => e.IsComposite
        ? new NavigationUserCompositeNode()
        {
          Id = NavigationId.Create(e.Id),
          Icon = e.Icon,
          PageType = typeof(HomePage),
          Title = e.Title,
          Position = e.Position,
          IsExpanded = e.IsExpanded
        }
      : new NavigationUserLeafNode()
      {
        Id = NavigationId.Create(e.Id),
        Icon = e.Icon,
        PageType = typeof(HomePage),
        Title = e.Title,
        Position = e.Position
      })
     .ToDictionary(n => n.Id.Value);
    nodes.Add(UserRootNavigation.Id.Value, UserRootNavigation);

    var families = entities
      .GroupBy(e => e.Parent)
      .ToDictionary(g => g.Key, g => new SortedSet<NavigationEntity>(g, Comparer<NavigationEntity>.Create((x, y) => x.Position.CompareTo(y.Position))));

    foreach (var family in families)
    {
      if (nodes.TryGetValue(family.Key, out var parent) && parent is NavigationUserCompositeNode compositeNode)
      {
        foreach (var childEntity in family.Value)
        {
          if (nodes.TryGetValue(childEntity.Id, out var childNode))
            compositeNode.ChildNodes.Add(childNode);
        }
      }
    }

    foreach (var node in nodes.Values)
      node.PropertyChanged += UserNode_PropertyChanged;
  }

  private async Task UpdateNavigationEntity(NavigationUserNode node, Action<NavigationEntity> action)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync();
    if (context.NavigationEntities.FirstOrDefault(e => e.Id == node.Id.Value) is NavigationEntity entity)
    {
      action.Invoke(entity);
      await context.SaveChangesAsync();
    }
  }

  public async Task<NavigationUserNode> AddUserNode(INavigationNode? navigation, bool isCompositeNode, Icon? iconName = null, string? title = null)
  {
    NavigationUserNode? node = navigation switch
    {
      NavigationUserLeafNode leaf => leaf,
      NavigationUserCompositeNode composite => composite.ChildNodes.LastOrDefault(),
      _ => UserRootNavigation.ChildNodes.LastOrDefault()
    };

    iconName ??= isCompositeNode ? Icon.System_Notebook : Icon.System_Board;
    title ??= isCompositeNode ? "Composite " + new Random().Next(10000) : "Leaf " + new Random().Next(10000);

    NavigationUserNode newNode = isCompositeNode
      ? new NavigationUserCompositeNode()
      {
        Id = NavigationId.NewId(),
        Icon = ((short)iconName).ToString(),
        Title = title,
        PageType = typeof(HomePage),
        Position = int.MaxValue,
        IsExpanded = true
      }
      : new NavigationUserLeafNode()
      {
        Id = NavigationId.NewId(),
        Icon = ((short)iconName).ToString(),
        Title = title,
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
      IsComposite = isCompositeNode,
      IsExpanded = isCompositeNode,
      IsDeleted = false
    };

    await using (var context = await DbContextFactory.CreateDbContextAsync())
    {
      await context.NavigationEntities.AddAsync(entity);
      await context.SaveChangesAsync();
    }

    newNode.PropertyChanged += UserNode_PropertyChanged;

    newNode.IsEditable = true;

    return newNode;
  }

  private async void UserNode_PropertyChanged(object? s, PropertyChangedEventArgs e)
  {
    if (s is NavigationUserNode node)
    {
      switch (e.PropertyName)
      {
        case nameof(NavigationUserNode.Position):
          await UpdateNavigationEntity(node, entity => entity.Position = node.Position);
          break;
        case nameof(NavigationUserNode.Title):
          await UpdateNavigationEntity(node, entity => entity.Title = node.Title);
          break;
        case nameof(NavigationUserCompositeNode.IsExpanded):
          if (node is NavigationUserCompositeNode compositeNode)
            await UpdateNavigationEntity(compositeNode, entity => entity.IsExpanded = compositeNode.IsExpanded);
          break;
      }
    }
  }



  private bool _disposed;
  public void Dispose()
  {
    if (_disposed)
      return;

    UserRootNavigation.ForEachDescendant(node => node.PropertyChanged -= UserNode_PropertyChanged);

    _disposed = true;
  }
}

internal sealed partial class NavigationService : IDisposable
{
  public Command<NavigationViewModelBase>? AddListCommand { get; private set; }
  public Command<NavigationViewModelBase>? AddGroupCommand { get; private set; }
  public Command<NavigationViewModelBase>? ShowAddNavigationDialogCommand { get; private set; }

  private void SetCommands()
  {
    AddListCommand = new(
      actionToExecute: async (vm) =>
      {
        if (vm.Navigation is INavigationNode navigation)
          await AddUserNode(navigation, false);
      },
      canExecuteFunc: vm => vm.Navigation is INavigationUserNode
    );

    AddGroupCommand = new(
      actionToExecute: async (vm) =>
      {
        if (vm.Navigation is INavigationNode navigation)
          await AddUserNode(navigation, true);
      },
      canExecuteFunc: vm => vm.Navigation is INavigationUserNode
    );

    ShowAddNavigationDialogCommand = new(
     actionToExecute: async (vm) =>
     {
       if (vm.Navigation is NavigationUserNode navigation
           && App.Instance.MainWindow?.Content.XamlRoot is XamlRoot xamlRoot)
       {
         var result = await DialogService.ShowAddNodeDialogAsync(xamlRoot, navigation);
         //if (result.ContentDialogResult == ContentDialogResult.Primary && result.AddedNavigation is NavigationUserNode addedNode)
        //  ViewModel.CurrentNavigation = addedNode;
       }
     }
   );
  }
}