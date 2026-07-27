using DotNext;

using MyNotes.Application.Commands.Navigations;
using MyNotes.Application.Contracts.Models.Navigations;
using MyNotes.Application.Services.Navigations;
using MyNotes.Common.Querying;
using MyNotes.Mappers;
using MyNotes.Models.Navigations;
using MyNotes.Shared.Enums.Navigations;
using MyNotes.Shared.Enums.Notes;

namespace MyNotes.Services.Navigations;

internal sealed partial class NavigationController : IDisposable
{
  public readonly NavigationService NavigationService;

  public ImmutableList<INavigation> PrimaryCoreNavigations { get; } = [NavigationHome.Instance, NavigationBookmarks.Instance, new NavigationSeparator()];
  public NavigationUserRootNode UserRootNavigation { get; } = NavigationUserRootNode.Instance;
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
    var rootTreeNodeDto = await NavigationService.Retrieval.BuildNavigationTreeAsync();
    NavigationMappers.ToModel(rootTreeNodeDto, UserRootNavigation);

    UserRootNavigation.ForEachDescendant(node =>
    {
      node.PropertyChanged += UserNode_PropertyChanged;
      node.PropertyChanged += node switch
      {
        NavigationUserCompositeNode => UserCompositeNode_PropertyChanged,
        NavigationUserLeafNode => UserLeafNode_PropertyChanged,
        _ => throw new InvalidOperationException()
      };
    }, false);
    InitializationTCS.TrySetResult();
  }

  public bool Disposed { get; private set; }

  public void Dispose()
  {
    if (Disposed)
    {
      return;
    }

    UserRootNavigation.ForEachDescendant(node =>
    {
      node.PropertyChanged -= UserNode_PropertyChanged;
      node.PropertyChanged -= UserCompositeNode_PropertyChanged;
      node.PropertyChanged -= UserLeafNode_PropertyChanged;
    }, false);

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

  private async void UserCompositeNode_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (sender is NavigationUserCompositeNode node)
    {
      switch (e.PropertyName)
      {
        case nameof(NavigationUserCompositeNode.IsExpanded):
          await NavigationService.Modification.UpdateNavigationViewStateAsync(new UpdateNavigationViewStateAppCommand()
          {
            PatchDto = new CompositeNavigationViewStatePatchDto()
            {
              Id = node.Id,
              IsExpanded = node.IsExpanded
            }
          });
          break;
      }
    }
  }

  private async void UserLeafNode_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (sender is NavigationUserLeafNode node)
    {
      switch (e.PropertyName)
      {
        case nameof(NavigationUserLeafNode.NoteSortKey):
          await NavigationService.Modification.UpdateNavigationViewStateAsync(new UpdateNavigationViewStateAppCommand()
          {
            PatchDto = new LeafNavigationViewStatePatchDto()
            {
              Id = node.Id,
              NoteSortKey = node.NoteSortKey ?? Optional<NoteSortKey>.None
            }
          });
          break;
        case nameof(NavigationUserLeafNode.NoteSortDirection):
          await NavigationService.Modification.UpdateNavigationViewStateAsync(new UpdateNavigationViewStateAppCommand()
          {
            PatchDto = new LeafNavigationViewStatePatchDto()
            {
              Id = node.Id,
              NoteSortDirection = node.NoteSortDirection ?? Optional<SortDirection>.None
            }
          });
          break;
        case nameof(NavigationUserLeafNode.PreviewLayoutType):
          await NavigationService.Modification.UpdateNavigationViewStateAsync(new UpdateNavigationViewStateAppCommand()
          {
            PatchDto = new LeafNavigationViewStatePatchDto()
            {
              Id = node.Id,
              PreviewLayoutType = node.PreviewLayoutType ?? Optional<PreviewLayoutType>.None
            }
          });
          break;
        case nameof(NavigationUserLeafNode.PreviewTileSize):
          await NavigationService.Modification.UpdateNavigationViewStateAsync(new UpdateNavigationViewStateAppCommand()
          {
            PatchDto = new LeafNavigationViewStatePatchDto()
            {
              Id = node.Id,
              PreviewTileSize = node.PreviewTileSize ?? Optional<PreviewTileSize>.None
            }
          });
          break;
        case nameof(NavigationUserLeafNode.PreviewTileRatio):
          await NavigationService.Modification.UpdateNavigationViewStateAsync(new UpdateNavigationViewStateAppCommand()
          {
            PatchDto = new LeafNavigationViewStatePatchDto()
            {
              Id = node.Id,
              PreviewTileRatio = node.PreviewTileRatio ?? Optional<PreviewTileRatio>.None
            }
          });
          break;
      }
    }
  }

  private async void UserNode_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    //await NavigationService.Modification.UpdateUserNavigationAsync();
    if (sender is NavigationUserNode node)
    {
      switch (e.PropertyName)
      {
        case nameof(NavigationUserNode.Parent):
          break;
        case nameof(NavigationUserNode.Icon):
          break;
        case nameof(NavigationUserNode.Title):
          break;
      }
    }
  }
}