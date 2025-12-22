using Microsoft.EntityFrameworkCore;

using MyNotes.Common.Commands;
using MyNotes.Models.Navigations;
using MyNotes.Services.Database;
using MyNotes.Services.Database.Entities;
using MyNotes.Services.Dialog;
using MyNotes.Templates;
using MyNotes.ViewModels.Navigations;
using MyNotes.Views.Navigations;
using MyNotes.Models.Modes;
using MyNotes.Services.Window;

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
    CurrentNavigation = PrimaryCoreNavigations[0];

    SetCommands();

    CurrentNavigationChanged += (s, e) => Console.WriteLine("{0}: {1}", "Current Navigation", (CurrentNavigation as INavigationNode)?.Title);
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
        && nodes.TryGetValue(omission.Id, out var childNode))
      {
        int position = Math.Clamp(omission.Position, 0, compositeNode.ChildNodes.Count);
        compositeNode.ChildNodes.Insert(position, childNode);
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
  private async Task<NavigationUserNode> AddUserNode(INavigationNode? targetNode, bool isCompositeNode, Icon? iconName = null, string? title = null)
  {
    NavigationUserNode? node = targetNode switch
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
        Icon = (short)iconName,
        Title = title,
        PageType = typeof(HomePage),
        Position = int.MaxValue,
        IsExpanded = true
      }
      : new NavigationUserLeafNode()
      {
        Id = NavigationId.NewId(),
        Icon = (short)iconName,
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
      parentNode = targetNode switch
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

  private async Task DeleteUserNode(NavigationUserNode node, DeleteMode deleteMode)
  {
    if (deleteMode == DeleteMode.Permanent)
    {

    }
    else if (deleteMode == DeleteMode.MoveToTrash)
    {
      node.TryFindRelations(out var parentNode, out var previousNode, out var nextNode);
      await UpdateNavigationEntity(node, entity =>
      {
        entity.RestorePrevious = previousNode?.Id.Value;
        entity.RestoreNext = nextNode?.Id.Value;
        entity.IsDeleted = true;
      });
      node.PropertyChanged -= UserNode_PropertyChanged;
      parentNode?.ChildNodes.Remove(node);
    }
  }
}

internal sealed partial class NavigationService : IDisposable
{
  public Command<NavigationViewModelBase>? AddListCommand { get; private set; }
  public Command<NavigationViewModelBase>? AddGroupCommand { get; private set; }
  public Command<NavigationViewModelBase>? ShowAddListDialogCommand { get; private set; }
  public Command<NavigationViewModelBase>? ShowAddGroupDialogCommand { get; private set; }
  public Command<NavigationViewModelBase>? ShowUpdateDialogCommand { get; private set; }
  public Command<NavigationViewModelBase>? ShowConfirmDeleteDialogCommand { get; private set; }

  private void SetCommands()
  {
    AddListCommand = new(
      actionToExecute: async (vm) =>
      {
        if (vm.Navigation is INavigationNode navigation)
        {
          ChangeCurrentNavigation(await AddUserNode(targetNode: navigation, isCompositeNode: false));
        }
      },
      canExecuteFunc: vm => vm.Navigation is INavigationUserNode
    );

    AddGroupCommand = new(
      actionToExecute: async (vm) =>
      {
        if (vm.Navigation is INavigationNode navigation)
          ChangeCurrentNavigation(await AddUserNode(targetNode: navigation, isCompositeNode: true));
      },
      canExecuteFunc: vm => vm.Navigation is INavigationUserNode
    );

    ShowAddListDialogCommand = new(
      actionToExecute: async (vm) =>
      {
        if (vm.Navigation is NavigationUserNode navigation
            && WindowService.MainWindow.Content.XamlRoot is XamlRoot xamlRoot)
        {
          var result = await DialogService.ShowEditUserNavigationDialogAsync(xamlRoot, navigation, EditMode.Create, false);
          if (result.ContentDialogResult == ContentDialogResult.Primary && result.Value is (Icon, string) v)
            ChangeCurrentNavigation(await AddUserNode(targetNode: navigation, isCompositeNode: false, iconName: v.Icon, title: v.Title));
        }
      },
      canExecuteFunc: vm => vm.Navigation is INavigationUserNode
    );

    ShowAddGroupDialogCommand = new(
      actionToExecute: async (vm) =>
      {
        if (vm.Navigation is NavigationUserNode navigation
            && WindowService.MainWindow.Content.XamlRoot is XamlRoot xamlRoot)
        {
          var result = await DialogService.ShowEditUserNavigationDialogAsync(xamlRoot, navigation, EditMode.Create, true);
          if (result.ContentDialogResult == ContentDialogResult.Primary && result.Value is (Icon, string) v)
            ChangeCurrentNavigation(await AddUserNode(targetNode: navigation, isCompositeNode: true, iconName: v.Icon, title: v.Title));
        }
      },
      canExecuteFunc: vm => vm.Navigation is INavigationUserNode
    );

    ShowUpdateDialogCommand = new(
      actionToExecute: async (vm) =>
      {
        if (vm.Navigation is NavigationUserNode navigation
            && WindowService.MainWindow.Content.XamlRoot is XamlRoot xamlRoot)
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
      }
    );

    ShowConfirmDeleteDialogCommand = new(
      actionToExecute: async (vm) =>
      {
        if (vm.Navigation is NavigationUserNode navigation
            && WindowService.MainWindow.Content.XamlRoot is XamlRoot xamlRoot)
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
      }
    );
  }
}