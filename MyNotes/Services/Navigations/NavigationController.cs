using MyNotes.Application.Dtos.Navigations.Common;
using MyNotes.Application.Services.Navigations;
using MyNotes.Mappers;
using MyNotes.Models.Navigations;

namespace MyNotes.Services.Navigations;

internal sealed partial class NavigationController : IDisposable
{
  public readonly NavigationService NavigationService;

  public ImmutableList<INavigation> PrimaryCoreNavigations { get; } = [NavigationHome.Instance, NavigationBookmarks.Instance, new NavigationSeparator()];
  public NavigationUserRootNode UserRootNavigation { get; } = new();
  public ImmutableList<INavigation> SecondaryCoreNavigations { get; } = [new NavigationSeparator(), NavigationTrash.Instance, NavigationSettings.Instance];
  public IReadOnlyList<NavigationUserNode> UserCompositeNavigations => UserRootNavigation.FindDescendants(node => node is NavigationUserCompositeNode, true);
  public IReadOnlyList<NavigationUserNode> UserLeafNavigations => UserRootNavigation.FindDescendants(node => node is NavigationUserLeafNode, false);

  private readonly TaskCompletionSource InitializationTCS = new();
  public Task InitializationTask => InitializationTCS.Task;

  #region Object Lifetime Management
  public NavigationController(NavigationService navigationService)
  {
    NavigationService = navigationService;
    _ = InitializeAsync();
  }

  private async Task InitializeAsync()
  {
    var rootBundleAppResponseDto = await NavigationService.Tree.BuildNavigationTreeAsync();
    foreach (var childDto in rootBundleAppResponseDto.Children)
    {
      UserRootNavigation.ChildNodes.Add(NavigationMappers.ToModel(childDto, UserRootNavigation));
    }
    InitializationTCS.TrySetResult();
  }

  public bool Disposed { get; private set; }

  public void Dispose()
  {
    if (Disposed)
    {
      return;
    }

    UserRootNavigation.ForEachDescendant(node => node.PropertyChanged -= UserNode_PropertyChanged);

    Disposed = true;
  }
  #endregion

  public INavigation? CurrentNavigation
  {
    get;
    private set
    {
      if (field != value)
      {
        field = value;
        CurrentNavigationChanged?.Invoke(this, value);
      }
    }
  }

  public event TypedEventHandler<object, INavigation?>? CurrentNavigationChanged;

  public Stack<INavigation> NavigationBackStack { get; } = new();

  public void NavigateTo(INavigation navigation)
  {
    if (CurrentNavigation != navigation)
    {
      if (CurrentNavigation is not null)
      {
        NavigationBackStack.Push(CurrentNavigation);
      }

      CurrentNavigation = navigation;
    }
  }

  public void NavigateBack()
  {
    if (NavigationBackStack.Count > 0)
    {
      CurrentNavigation = NavigationBackStack.Pop();
    }
  }

  public void ResetNavigation()
  {
    NavigationBackStack.Clear();
    CurrentNavigation = null;
  }

  private async void UserNode_PropertyChanged(object? s, PropertyChangedEventArgs e)
  {
    //await NavigationService.Modification.UpdateUserNavigationAsync();
#if false
    if (s is NavigationUserNode node)
    {
      switch (e.PropertyName)
      {
        case nameof(NavigationUserNode.Parent):
          await UpdateNavigationEntityAsync(node, entity => entity.Parent = node.Parent.Id.Value);
          break;
        case nameof(NavigationUserNode.Icon):
          await UpdateNavigationEntityAsync(node, entity => entity.Icon = (int)node.Icon);
          break;
        case nameof(NavigationUserNode.Title):
          await UpdateNavigationEntityAsync(node, entity => entity.Title = node.Title);
          break;
        case nameof(NavigationUserNode.Position):
          await UpdateNavigationEntityAsync(node, entity => entity.Position = node.Position);
          break;
        case nameof(NavigationUserCompositeNode.IsExpanded):
          if (node is NavigationUserCompositeNode compositeNodeIE)
          {
            await UpdateNavigationEntityAsync(compositeNodeIE, entity => entity.IsExpanded = compositeNodeIE.IsExpanded);
          }
          break;
        case nameof(NavigationUserLeafNode.NoteSortKey):
          if (node is NavigationUserLeafNode leafNodeNSK)
          {
            await UpdateNavigationEntityAsync(leafNodeNSK, entity => entity.NoteSortKey = leafNodeNSK.NoteSortKey.AsInt());
          }
          break;
        case nameof(NavigationUserLeafNode.NoteSortDirection):
          if (node is NavigationUserLeafNode leafNodeNSD)
          {
            await UpdateNavigationEntityAsync(leafNodeNSD, entity => entity.NoteSortDirection = leafNodeNSD.NoteSortDirection.AsInt());
          }
          break;
        case nameof(NavigationUserLeafNode.PreviewLayoutType):
          if (node is NavigationUserLeafNode leafNodePLT)
          {
            await UpdateNavigationEntityAsync(leafNodePLT, entity => entity.PreviewLayoutType = leafNodePLT.PreviewLayoutType.AsInt());
          }
          break;
        case nameof(NavigationUserLeafNode.PreviewTileSize):
          if (node is NavigationUserLeafNode leafNodePTS)
          {
            await UpdateNavigationEntityAsync(leafNodePTS, entity => entity.PreviewTileSize = leafNodePTS.PreviewTileSize.AsInt());
          }
          break;
        case nameof(NavigationUserLeafNode.PreviewTileRatio):
          if (node is NavigationUserLeafNode leafNodePTR)
          {
            await UpdateNavigationEntityAsync(leafNodePTR, entity => entity.PreviewTileRatio = leafNodePTR.PreviewTileRatio.AsInt());
          }
          break;
      }
    }
#endif
  }
}