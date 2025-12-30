using Microsoft.EntityFrameworkCore;

using MyNotes.Common.Commands;
using MyNotes.Models.Modes;
using MyNotes.Models.Navigations;
using MyNotes.Services.Database;
using MyNotes.Services.Database.Entities;
using MyNotes.Services.Dialog;
using MyNotes.Services.Window;
using MyNotes.Templates;
using MyNotes.Views.Navigations;

namespace MyNotes.Services.Navigation;

internal sealed partial class NavigationService : IDisposable
{
  private readonly IDbContextFactory<AppDbContext> DbContextFactory;
  private readonly WindowService WindowService;
  private readonly DialogService DialogService;

  public ImmutableList<INavigation> PrimaryCoreNavigations { get; } = [NavigationHome.Instance, NavigationBookmarks.Instance, new NavigationSeparator()];
  public NavigationUserRootNode UserRootNavigation { get; } = NavigationUserRootNode.Instance;
  public ImmutableList<INavigation> SecondaryCoreNavigations { get; } = [new NavigationSeparator(), NavigationTrash.Instance, NavigationSettings.Instance];

  public INavigation? CurrentNavigation { get; private set; }
  public Stack<INavigation> NavigationBackStack { get; } = new();

  public event TypedEventHandler<object, INavigation>? CurrentNavigationChanged;

  public NavigationService(IDbContextFactory<AppDbContext> dbContextFactory, WindowService windowService, DialogService dialogService)
  {
    DbContextFactory = dbContextFactory;
    WindowService = windowService;
    DialogService = dialogService;

    BuildNavigationTask = BuildNavigationTree();

    SetCommands();
  }

  private bool _disposed;
  public void Dispose()
  {
    if (_disposed)
      return;

    UserRootNavigation.ForEachDescendant(node => node.PropertyChanged -= UserNode_PropertyChanged);

    _disposed = true;
  }

  public Task BuildNavigationTask { get; }
  private async Task BuildNavigationTree()
  {
    await using var context = await DbContextFactory.CreateDbContextAsync();
    var entities = await context.NavigationEntities.ToListAsync();
    var nodes = entities
      .Select<NavigationEntity, NavigationUserNode>(e => e.IsComposite
        ? new NavigationUserCompositeNode()
        {
          Id = NavigationId.Create(e.Id),
          Parent = null!,
          Icon = e.Icon,
          PageType = typeof(HomePage),
          Title = e.Title,
          Position = e.Position,
          IsExpanded = e.IsExpanded
        }
      : new NavigationUserLeafNode()
      {
        Id = NavigationId.Create(e.Id),
        Parent = null!,
        Icon = e.Icon,
        PageType = typeof(HomePage),
        Title = e.Title,
        Position = e.Position
      })
     .ToDictionary(n => n.Id.Value);

    HashSet<NavigationEntity> omissions = [.. entities];

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
          {
            if (!childEntity.IsDeleted)
              compositeNode.ChildNodes.Add(childNode);
            omissions.Remove(childEntity);
          }
        }
      }
    }

    foreach (var node in nodes.Values)
      node.PropertyChanged += UserNode_PropertyChanged;

    foreach (var omission in omissions)
    {
      if (nodes.TryGetValue(omission.Parent, out var parentNode)
        && parentNode is NavigationUserCompositeNode compositeNode
        && nodes.TryGetValue(omission.Id, out var omitNode))
      {
        var childNodes = compositeNode.ChildNodes;
        var pivot = childNodes.FirstOrDefault(n => n.Position > omitNode.Position);
        int index = pivot is null
          ? childNodes.Count == 0 || omitNode.Position <= childNodes[0].Position ? 0 : childNodes.Count
          : childNodes.IndexOf(pivot);
        compositeNode.ChildNodes.Insert(index, omitNode);
      }
    }
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

  private async void UserNode_PropertyChanged(object? s, PropertyChangedEventArgs e)
  {
    if (s is NavigationUserNode node)
    {
      switch (e.PropertyName)
      {
        case nameof(NavigationUserNode.Parent):
          await UpdateNavigationEntity(node, entity => entity.Parent = node.Parent.Id.Value);
          break;
        case nameof(NavigationUserNode.Position):
          Console.WriteLine("{0}: {1}", node.Title, node.Position);
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

  private void ChangeCurrentNavigation(INavigation navigation)
  {
    CurrentNavigation = navigation;
    CurrentNavigationChanged?.Invoke(this, navigation);
  }

  public void PushNavigationBackStack(INavigation navigation)
  {
    if (CurrentNavigation is not null)
      NavigationBackStack.Push(CurrentNavigation);
    ChangeCurrentNavigation(navigation);
  }

  public void PopNavigationBackStack()
  {
    if (NavigationBackStack.Count > 0)
    {
      ChangeCurrentNavigation(NavigationBackStack.Pop());
    }
  }
}

internal sealed partial class NavigationService : IDisposable
{
  private async Task<NavigationUserNode> AddUserNode(INavigationNode? targetNode, bool isCompositeNode, Icon iconName, string title)
  {
    NavigationUserNode? beforeNode = targetNode switch
    {
      NavigationUserLeafNode leaf => leaf,
      NavigationUserCompositeNode composite => composite.ChildNodes.LastOrDefault(),
      _ => UserRootNavigation.ChildNodes.LastOrDefault()
    };

    NavigationUserCompositeNode parentNode = beforeNode is null
      ? targetNode switch
      {
        NavigationUserLeafNode leaf => leaf.Parent,
        NavigationUserCompositeNode composite => composite,
        _ => UserRootNavigation
      }
      : beforeNode.Parent;

    NavigationUserNode newNode = isCompositeNode
      ? new NavigationUserCompositeNode()
      {
        Id = NavigationId.NewId(),
        Parent = parentNode,
        Icon = (short)iconName,
        Title = title,
        PageType = typeof(HomePage),
        Position = int.MaxValue,
        IsExpanded = true
      }
      : new NavigationUserLeafNode()
      {
        Id = NavigationId.NewId(),
        Parent = parentNode,
        Icon = (short)iconName,
        Title = title,
        PageType = typeof(HomePage),
        Position = int.MaxValue
      };

    int index = beforeNode is null ? parentNode.ChildNodes.Count : parentNode.ChildNodes.IndexOf(beforeNode) + 1;
    parentNode.ChildNodes.Insert(index, newNode);

    NavigationEntity entity = new()
    {
      Id = newNode.Id.Value,
      Title = newNode.Title,
      Icon = newNode.Icon,
      Parent = newNode.Parent.Id.Value,
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

  private async Task DeleteUserNode(NavigationUserNode node, DeleteMode deleteMode)
  {
    if (deleteMode == DeleteMode.Permanent)
    {

    }
    else if (deleteMode == DeleteMode.MoveToTrash)
    {
      await UpdateNavigationEntity(node, entity =>
      {
        entity.RestorePrevious = node.FindPreviousNode()?.Id.Value;
        entity.RestoreNext = node.FindNextNode()?.Id.Value;
        entity.IsDeleted = true;
      });
      if (node is NavigationUserCompositeNode compositeNode)
      {
        var childNodes = compositeNode.ChildNodes;
        while (childNodes.Count > 0)
        {
          var childNode = childNodes[^1];
          await DeleteUserNode(childNode, deleteMode);
        }
      }
      node.PropertyChanged -= UserNode_PropertyChanged;
      node.Parent.ChildNodes.Remove(node);
    }
  }
}

internal sealed partial class NavigationService : IDisposable
{
  public Command<NavigationUserNode>? AddListCommand { get; private set; }
  public Command<NavigationUserNode>? AddGroupCommand { get; private set; }
  public Command<NavigationUserNode>? UpdateCommand { get; private set; }
  public Command<NavigationUserNode>? DeleteCommand { get; private set; }
  public Command<(NavigationUserNode SourceItem, NavigationUserCompositeNode TargetGroup)>? MoveToGroupCommand { get; private set; }

  private void SetCommands()
  {
    AddListCommand = new(
      actionToExecute: async (targetNavigation) =>
      {
        if (targetNavigation is NavigationUserNode navigation
            && WindowService.MainWindow is not null
            && WindowService.MainWindow.TryGetTarget(out var mainWindow)
            && mainWindow.Content.XamlRoot is XamlRoot xamlRoot)
        {
          var result = await DialogService.ShowEditUserNavigationDialogAsync(xamlRoot, navigation, EditMode.Create, false);
          if (result.ContentDialogResult == ContentDialogResult.Primary && result.Value is (Icon, string) v)
            ChangeCurrentNavigation(await AddUserNode(targetNode: navigation, isCompositeNode: false, iconName: v.Icon, title: v.Title));
        }
      });

    AddGroupCommand = new(
      actionToExecute: async (targetNavigation) =>
      {
        if (targetNavigation is NavigationUserNode navigation
            && WindowService.MainWindow is not null
            && WindowService.MainWindow.TryGetTarget(out var mainWindow)
            && mainWindow.Content.XamlRoot is XamlRoot xamlRoot)
        {
          var result = await DialogService.ShowEditUserNavigationDialogAsync(xamlRoot, navigation, EditMode.Create, true);
          if (result.ContentDialogResult == ContentDialogResult.Primary && result.Value is (Icon, string) v)
            ChangeCurrentNavigation(await AddUserNode(targetNode: navigation, isCompositeNode: true, iconName: v.Icon, title: v.Title));
        }
      });

    UpdateCommand = new(
      actionToExecute: async (targetNavigation) =>
      {
        if (targetNavigation is NavigationUserNode navigation
            && WindowService.MainWindow is not null
            && WindowService.MainWindow.TryGetTarget(out var mainWindow)
            && mainWindow.Content.XamlRoot is XamlRoot xamlRoot)
        {
          var result = await DialogService.ShowEditUserNavigationDialogAsync(xamlRoot, navigation, EditMode.Update, navigation is NavigationUserCompositeNode);
          if (result.ContentDialogResult == ContentDialogResult.Primary && result.Value is (Icon, string) v)
          {
            short icon = (short)v.Icon;
            string title = v.Title;


            navigation.Icon = icon;
            navigation.Title = title;

            await UpdateNavigationEntity(navigation, e =>
            {
              e.Icon = icon;
              e.Title = title;
            });
          }
        }
      });

    DeleteCommand = new(
      actionToExecute: async (targetNavigation) =>
      {
        if (targetNavigation is NavigationUserNode navigation
            && WindowService.MainWindow is not null
            && WindowService.MainWindow.TryGetTarget(out var mainWindow)
            && mainWindow.Content.XamlRoot is XamlRoot xamlRoot)
        {
          var targetTypeName = navigation switch
          {
            NavigationUserLeafNode => "List",
            NavigationUserCompositeNode => "Group",
            _ => string.Empty
          };
          var deleteMode = DeleteMode.MoveToTrash;
          if (await DialogService.ShowConfirmDeleteDialogAsync(xamlRoot, targetTypeName, navigation.Title, deleteMode) == ContentDialogResult.Primary)
          {
            await DeleteUserNode(navigation, deleteMode);
          }
        }
      });

    MoveToGroupCommand = new(
      actionToExecute: (parameter) =>
      {
        NavigationUserNode sourceItem = parameter.SourceItem;
        NavigationUserCompositeNode targetGroup = parameter.TargetGroup;

        if (sourceItem.Parent != targetGroup)
        {
          sourceItem.Parent.ChildNodes.Remove(sourceItem);
          targetGroup.ChildNodes.Add(sourceItem);
        }

      });
  }
}