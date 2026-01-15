using Microsoft.EntityFrameworkCore;

using MyNotes.Common.Structures;
using MyNotes.Constants;
using MyNotes.Models.Modes;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.Database;
using MyNotes.Services.Database.Entities;
using MyNotes.Services.Settings;
using MyNotes.Templates;

namespace MyNotes.Services.Navigations;

internal sealed partial class NavigationService : IDisposable
{
  private readonly IDbContextFactory<AppDbContext> DbContextFactory;
  private readonly SettingsService SettingsService;

  public ImmutableList<INavigation> PrimaryCoreNavigations { get; } = [NavigationHome.Instance, NavigationBookmarks.Instance, new NavigationSeparator()];
  public NavigationUserRootNode UserRootNavigation { get; } = NavigationUserRootNode.Instance;
  public ImmutableList<INavigation> SecondaryCoreNavigations { get; } = [new NavigationSeparator(), NavigationTrash.Instance, NavigationSettings.Instance];

  public INavigation? CurrentNavigation { get; private set; }
  public Stack<INavigation> NavigationBackStack { get; } = new();

  public event TypedEventHandler<object, INavigation>? CurrentNavigationChanged;

  public NavigationService(IDbContextFactory<AppDbContext> dbContextFactory, SettingsService settingsService)
  {
    DbContextFactory = dbContextFactory;
    SettingsService = settingsService;

    BuildNavigationTask = BuildNavigationTree();
  }

  public bool IsDisposed => _disposed;

  private bool _disposed;
  public void Dispose()
  {
    if (_disposed)
      return;

    UserRootNavigation.ForEachDescendant(node => node.PropertyChanged -= UserNode_PropertyChanged);

    _disposed = true;
  }

  #region Build Navigation Tree (Initialize)
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
          Icon = (Icon)e.Icon,
          Title = e.Title,
          Position = e.Position,
          IsExpanded = e.IsExpanded
        }
      : new NavigationUserLeafNode()
      {
        Id = NavigationId.Create(e.Id),
        Parent = null!,
        Icon = (Icon)e.Icon,
        Title = e.Title,
        Position = e.Position,
        NoteSortKey = (NoteSortKey)(e.NoteSortKey ?? SettingsService.Load(SettingsDescriptors.NoteSortKey)),
        NoteSortDirection = (SortDirection)(e.NoteSortDirection ?? SettingsService.Load(SettingsDescriptors.NoteSortDirection))
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
  #endregion

  private async void UserNode_PropertyChanged(object? s, PropertyChangedEventArgs e)
  {
    if (s is NavigationUserNode node)
    {
      switch (e.PropertyName)
      {
        case nameof(NavigationUserNode.Parent):
          await UpdateNavigationEntityAsync(node, entity => entity.Parent = node.Parent.Id.Value);
          break;
        case nameof(NavigationUserNode.Icon):
          await UpdateNavigationEntityAsync(node, entity => entity.Icon = (short)node.Icon);
          break;
        case nameof(NavigationUserNode.Title):
          await UpdateNavigationEntityAsync(node, entity => entity.Title = node.Title);
          break;
        case nameof(NavigationUserNode.Position):
          await UpdateNavigationEntityAsync(node, entity => entity.Position = node.Position);
          break;
        case nameof(NavigationUserCompositeNode.IsExpanded):
          if (node is NavigationUserCompositeNode compositeNodeIE)
            await UpdateNavigationEntityAsync(compositeNodeIE, entity => entity.IsExpanded = compositeNodeIE.IsExpanded);
          break;
        case nameof(NavigationUserLeafNode.NoteSortKey):
          if (node is NavigationUserLeafNode leafNodeNSK)
            await UpdateNavigationEntityAsync(leafNodeNSK, entity => entity.NoteSortKey = (int)leafNodeNSK.NoteSortKey);
          break;
        case nameof(NavigationUserLeafNode.NoteSortDirection):
          if (node is NavigationUserLeafNode leafNodeNSD)
            await UpdateNavigationEntityAsync(leafNodeNSD, entity => entity.NoteSortDirection = (int)leafNodeNSD.NoteSortDirection);
          break;
      }
    }
  }

  public void ChangeCurrentNavigation(INavigation navigation)
  {
    CurrentNavigation = navigation;
    CurrentNavigationChanged?.Invoke(this, navigation);
  }

  public void ResetCurrentNavigation() => CurrentNavigation = null;

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

#region Models & Entities (CRUD)
internal sealed partial class NavigationService : IDisposable
{
  // Navigation 속성 변경 사항 DB에 반영
  public async Task UpdateNavigationEntityAsync(NavigationUserNode node, Action<NavigationEntity> action)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync();
    if (context.NavigationEntities.Find(node.Id.Value) is NavigationEntity entity)
    {
      action.Invoke(entity);
      await context.SaveChangesAsync();
    }
  }

  // Navigation 인스턴스 생성 및 DB 테이블에 추가
  public async Task<NavigationUserNode?> AddUserNodeAsync(INavigationNode? targetNode, bool isCompositeNode, Icon icon, string title)
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
        Icon = icon,
        Title = title,
        Position = int.MaxValue,
        IsExpanded = true
      }
      : new NavigationUserLeafNode()
      {
        Id = NavigationId.NewId(),
        Parent = parentNode,
        Icon = icon,
        Title = title,
        Position = int.MaxValue,
        NoteSortKey = (NoteSortKey)SettingsService.Load(SettingsDescriptors.NoteSortKey),
        NoteSortDirection = (SortDirection)SettingsService.Load(SettingsDescriptors.NoteSortDirection)
      };

    await using (var context = await DbContextFactory.CreateDbContextAsync())
    {
      if (!await context.NavigationEntities.AnyAsync(e => e.Id == newNode.Id.Value))
      {
        int index = beforeNode is null ? parentNode.ChildNodes.Count : parentNode.ChildNodes.IndexOf(beforeNode) + 1;
        parentNode.ChildNodes.Insert(index, newNode);
        newNode.PropertyChanged += UserNode_PropertyChanged;

        NavigationEntity entity = new()
        {
          Id = newNode.Id.Value,
          Title = newNode.Title,
          Icon = (short)newNode.Icon,
          Parent = newNode.Parent.Id.Value,
          Position = newNode.Position,
          IsComposite = isCompositeNode,
          IsExpanded = isCompositeNode,
          IsDeleted = false
        };

        await context.NavigationEntities.AddAsync(entity);
        await context.SaveChangesAsync();

        return newNode;
      }
    }

    return null;
  }

  // Navigation 삭제 및 DB 테이블에 반영
  public async Task DeleteUserNodeAsync(NavigationUserNode node, DeleteMode deleteMode)
  {
    if (deleteMode == DeleteMode.Permanent)
    {

    }
    else if (deleteMode == DeleteMode.MoveToTrash)
    {
      await UpdateNavigationEntityAsync(node, entity =>
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
          await DeleteUserNodeAsync(childNode, deleteMode);
        }
      }
      node.PropertyChanged -= UserNode_PropertyChanged;
      node.Parent.ChildNodes.Remove(node);
    }
  }
}
#endregion