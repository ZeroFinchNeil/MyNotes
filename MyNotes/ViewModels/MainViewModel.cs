using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Media.Imaging;

using MyNotes.Models.Navigation;
using MyNotes.Services.Database;
using MyNotes.Services.Database.Entities;
using MyNotes.Views.Navigations;

namespace MyNotes.ViewModels;

internal sealed partial class MainViewModel : DisposableViewModelBase
{
  private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

  private readonly ImmutableList<INavigation> PrimaryCoreNavigations;
  public NavigationUserRootNode UserRootNavigation { get; } = NavigationUserRootNode.Instance;
  private readonly ImmutableList<INavigation> SecondaryCoreNavigations;

  public CollectionViewSource MenuItems { get; } = new() { IsSourceGrouped = true };
  public IReadOnlyList<INavigation> FooterMenuItems => SecondaryCoreNavigations;
  public IReadOnlyList<INavigationUserNode> UserNavigations => UserRootNavigation.ChildNodes;
  
  public INavigation? CurrentNavigation
  {
    get;
    set => SetProperty(ref field, value);
  }

  public MainViewModel(IDbContextFactory<AppDbContext> dbContextFactory)
  {
    _dbContextFactory = dbContextFactory;
    SetCommands();

    PrimaryCoreNavigations = [NavigationHome.Instance, NavigationBookmarks.Instance, new NavigationSeparator()];
    SecondaryCoreNavigations = [new NavigationSeparator(), NavigationTrash.Instance, NavigationSettings.Instance];

    IReadOnlyList<IReadOnlyList<INavigation>> MenuItemsSource = [PrimaryCoreNavigations, UserNavigations];
    MenuItems.Source = MenuItemsSource;
    CurrentNavigation = PrimaryCoreNavigations[0];

    _ = BuildNavigationTree();
  }

  protected override void Dispose(bool disposing)
  {
    if (_disposed)
      return;

    if (disposing)
    {
      UserRootNavigation.ForEachDescendant(node => node.PropertyChanged -= UserNode_PropertyChanged);
    }

    _disposed = true;
  }

  public async Task BuildNavigationTree()
  {
    await using var context = await _dbContextFactory.CreateDbContextAsync();
    var entities = context.NavigationEntities.AsEnumerable();
    var nodes = entities
      .Select<NavigationEntity, NavigationUserNode>(e => e.IsComposite
        ? new NavigationUserCompositeNode()
        {
          Id = NavigationId.Create(e.Id),
          Icon = e.Icon,
          PageType = typeof(HomePage),
          Title = e.Title,
          Position = e.Position
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
      if(nodes.TryGetValue(family.Key, out var parent) && parent is NavigationUserCompositeNode compositeNode)
      {
        foreach(var childEntity in family.Value)
        {
          if (nodes.TryGetValue(childEntity.Id, out var childNode))
            compositeNode.ChildNodes.Add(childNode);
        }
      }
    }

    foreach (var node in nodes.Values)
      node.PropertyChanged += UserNode_PropertyChanged;
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
      }
    }
  }

  private async Task UpdateNavigationEntity(NavigationUserNode node, Action<NavigationEntity> action)
  {
    await using var context = await _dbContextFactory.CreateDbContextAsync();
    if (context.NavigationEntities.FirstOrDefault(e => e.Id == node.Id.Value) is NavigationEntity entity)
    {
      action.Invoke(entity);
      await context.SaveChangesAsync();
    }
  }
}