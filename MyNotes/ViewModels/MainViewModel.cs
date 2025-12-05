using Microsoft.EntityFrameworkCore;

using MyNotes.Models.Navigation;
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

    IReadOnlyList<IReadOnlyList<INavigation>> MenuItemsSource = [PrimaryCoreNavigations, UserNavigations];
    MenuItems.Source = MenuItemsSource;

    _ = BuildNavigationTree();
  }

  public async Task BuildNavigationTree()
  {
    await using var context = await _dbContextFactory.CreateDbContextAsync();

    // Dictionary<ParentNode, Dictionary<ChildId, ChildEntity>>
    // Entity로부터 부모 노드 복원 및 공통 부모로 그룹핑
    var group = context.NavigationEntities
      .AsEnumerable()
      .GroupBy(e => e.Parent == UserRootNavigation.Id.Value
        ? UserRootNavigation
        : new NavigationUserCompositeNode()
        {
          Id = NavigationId.Create(e.Id),
          Icon = new SymbolIconSource() { Symbol = Symbol.List },
          PageType = typeof(HomePage),
          Title = e.Title,
          Position = e.Position
        })
      .ToDictionary(g => g.Key, g => g.ToDictionary(e => e.Id));

    // 부모 노드 빠른 탐색용
    Dictionary<Guid, NavigationUserCompositeNode> parents = group.Keys.ToDictionary(e => e.Id.Value);

    Debug.WriteLine(parents.Count);
    foreach (var family in group)
    {
      // family => Key: ParentNode, Value: Dictionary<ChildId, ChildEntity>
      // parents => Key: ParentId, Value: ParentNode

      var parentNode = family.Key;
      var children = family.Value;

      Debug.WriteLine(parentNode.Id.Value);
      Debug.WriteLine(children.Count);

      parentNode.PropertyChanged += UserNode_PropertyChanged;

      List<NavigationUserNode> childNodes = new();
      // child => Key: ChildId, Value: ChildEntity 
      foreach (var child in children)
      {
        // Entity로부터 자식 노드 복원(이미 복원된 부모 노드는 기존 노드 사용)
        var childId = child.Key;
        var childEntity = child.Value;
        NavigationUserNode childNode = parents.TryGetValue(child.Key, out var existingNode)
          ? existingNode
          : childEntity.IsComposite
             ? new NavigationUserCompositeNode()
             {
               Id = NavigationId.Create(childEntity.Id),
               Icon = new SymbolIconSource() { Symbol = Symbol.List },
               PageType = typeof(HomePage),
               Title = childEntity.Title,
               Position = childEntity.Position
             }
            : new NavigationUserLeafNode()
            {
              Id = NavigationId.Create(childEntity.Id),
              Icon = new SymbolIconSource() { Symbol = Symbol.List },
              PageType = typeof(HomePage),
              Title = childEntity.Title,
              Position = childEntity.Position
            };

        if (!childNode.Equals(existingNode))
          childNode.PropertyChanged += UserNode_PropertyChanged;

        childNodes.Add(childNode);
      }

      // 순서에 맞게 자식 노드 정렬
      foreach (var child in childNodes.OrderBy(n => n.Position))
      {
        parentNode.ChildNodes.Add(child);
      }
    }
  }

  private async void UserNode_PropertyChanged(object? s, PropertyChangedEventArgs e)
  {
    if (s is NavigationUserNode node)
    {
      if (e.PropertyName == nameof(NavigationUserNode.Position))
      {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        if (context.NavigationEntities.FirstOrDefault(e => e.Id == node.Id.Value) is NavigationEntity entity)
        {
          entity.Position = node.Position;
          await context.SaveChangesAsync();
        }
      }
    }
  }
}