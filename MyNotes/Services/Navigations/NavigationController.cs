using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Querying.Models;
using MyNotes.Application.Navigations;
using MyNotes.Application.Navigations.Commands;
using MyNotes.Application.Navigations.Services;
using MyNotes.Application.Settings.Services;
using MyNotes.Common.Mappers;
using MyNotes.Models.Navigations;

namespace MyNotes.Services.Navigations;

internal sealed partial class NavigationController : IDisposable
{
  private readonly NavigationService NavigationService;
  private readonly AppSettingsService AppSettingsService;

  public ImmutableList<INavigation> PrimaryCoreNavigations { get; } = [NavigationHome.Instance, NavigationBookmarks.Instance, new NavigationSeparator()];
  public NavigationUserRootNode UserRootNavigation { get; } = NavigationUserRootNode.Instance;
  public ImmutableList<INavigation> SecondaryCoreNavigations { get; } = [new NavigationSeparator(), NavigationTrash.Instance, NavigationSettings.Instance];
  public IReadOnlyList<NavigationUserNode> UserCompositeNavigations => UserRootNavigation.FindDescendants(node => node is NavigationUserCompositeNode, true);
  public IReadOnlyList<NavigationUserNode> UserLeafNavigations => UserRootNavigation.FindDescendants(node => node is NavigationUserLeafNode, false);

  public Task InitializationTask { get; }

  #region Object Lifetime Management
  public NavigationController(NavigationService navigationService, AppSettingsService appSettingsService)
  {
    NavigationService = navigationService;
    AppSettingsService = appSettingsService;
    InitializationTask = InitializeAsync();
  }

  private async Task InitializeAsync()
  {
    InitializeNavigationBookmarks();
    InitializeNavigationTrash();

    await BuildNavigationTree();
  }

  private void InitializeNavigationBookmarks()
  {
    NavigationBookmarks.Instance.NoteSortKey = AppSettingsService.Load<NoteSortKey, int>(NoteSortKeySettingsCodec.Decode, NavigationSettingsDescriptors.BookmarksPageNoteSortKey);
    NavigationBookmarks.Instance.NoteSortDirection = AppSettingsService.Load<SortDirection, int>(SortDirectionSettingsCodec.Decode, NavigationSettingsDescriptors.BookmarksPageNoteSortDirection);
    NavigationBookmarks.Instance.PreviewLayoutType = AppSettingsService.Load<PreviewLayoutType, int>(PreviewLayoutTypeSettingsCodec.Decode, NavigationSettingsDescriptors.BookmarksPagePreviewLayoutType);
    NavigationBookmarks.Instance.PreviewTileSize = AppSettingsService.Load<PreviewTileSize, int>(PreviewTileSizeSettingsCodec.Decode, NavigationSettingsDescriptors.BookmarksPagePreviewTileSize);
    NavigationBookmarks.Instance.PreviewTileRatio = AppSettingsService.Load<PreviewTileRatio, int>(PreviewTileRatioSettingsCodec.Decode, NavigationSettingsDescriptors.BookmarksPagePreviewTileRatio);
    NavigationBookmarks.Instance.PropertyChanged += NavigationBookmarks_PropertyChanged;
  }

  private void InitializeNavigationTrash()
  {
    NavigationTrash.Instance.NoteSortKey = AppSettingsService.Load<NoteSortKey, int>(NoteSortKeySettingsCodec.Decode, NavigationSettingsDescriptors.TrashPageNoteSortKey);
    NavigationTrash.Instance.NoteSortDirection = AppSettingsService.Load<SortDirection, int>(SortDirectionSettingsCodec.Decode, NavigationSettingsDescriptors.TrashPageNoteSortDirection);
    NavigationTrash.Instance.PreviewLayoutType = AppSettingsService.Load<PreviewLayoutType, int>(PreviewLayoutTypeSettingsCodec.Decode, NavigationSettingsDescriptors.TrashPagePreviewLayoutType);
    NavigationTrash.Instance.PreviewTileSize = AppSettingsService.Load<PreviewTileSize, int>(PreviewTileSizeSettingsCodec.Decode, NavigationSettingsDescriptors.TrashPagePreviewTileSize);
    NavigationTrash.Instance.PreviewTileRatio = AppSettingsService.Load<PreviewTileRatio, int>(PreviewTileRatioSettingsCodec.Decode, NavigationSettingsDescriptors.TrashPagePreviewTileRatio);
    NavigationTrash.Instance.PropertyChanged += NavigationTrash_PropertyChanged;
  }

  private async Task BuildNavigationTree()
  {
    var rootTreeNodeDto = await NavigationService.Retrieval.BuildNavigationTreeAsync();
    Common.Mappers.NavigationMappers.ToModel(rootTreeNodeDto, UserRootNavigation);

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
  }

  public bool Disposed { get; private set; }

  public void Dispose()
  {
    if (Disposed)
    {
      return;
    }
    NavigationBookmarks.Instance.PropertyChanged -= NavigationBookmarks_PropertyChanged;
    NavigationTrash.Instance.PropertyChanged -= NavigationTrash_PropertyChanged;

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

  private void NavigationBookmarks_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (sender is INavigationNoteList node)
    {
      switch (e.PropertyName)
      {
        case nameof(INavigationNoteList.NoteSortKey):
          AppSettingsService.Save(NoteSortKeySettingsCodec.Encode, NavigationSettingsDescriptors.BookmarksPageNoteSortKey, node.NoteSortKey);
          break;
        case nameof(INavigationNoteList.NoteSortDirection):
          AppSettingsService.Save(SortDirectionSettingsCodec.Encode, NavigationSettingsDescriptors.BookmarksPageNoteSortDirection, node.NoteSortDirection);
          break;
        case nameof(INavigationNoteList.PreviewLayoutType):
          AppSettingsService.Save(PreviewLayoutTypeSettingsCodec.Encode, NavigationSettingsDescriptors.BookmarksPagePreviewLayoutType, node.PreviewLayoutType);
          break;
        case nameof(INavigationNoteList.PreviewTileSize):
          AppSettingsService.Save(PreviewTileSizeSettingsCodec.Encode, NavigationSettingsDescriptors.BookmarksPagePreviewTileSize, node.PreviewTileSize);
          break;
        case nameof(INavigationNoteList.PreviewTileRatio):
          AppSettingsService.Save(PreviewTileRatioSettingsCodec.Encode, NavigationSettingsDescriptors.BookmarksPagePreviewTileRatio, node.PreviewTileRatio);
          break;
      }
    }
  }

  private void NavigationTrash_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (sender is INavigationNoteList node)
    {
      switch (e.PropertyName)
      {
        case nameof(INavigationNoteList.NoteSortKey):
          AppSettingsService.Save(NoteSortKeySettingsCodec.Encode, NavigationSettingsDescriptors.TrashPageNoteSortKey, node.NoteSortKey);
          break;
        case nameof(INavigationNoteList.NoteSortDirection):
          AppSettingsService.Save(SortDirectionSettingsCodec.Encode, NavigationSettingsDescriptors.TrashPageNoteSortDirection, node.NoteSortDirection);
          break;
        case nameof(INavigationNoteList.PreviewLayoutType):
          AppSettingsService.Save(PreviewLayoutTypeSettingsCodec.Encode, NavigationSettingsDescriptors.TrashPagePreviewLayoutType, node.PreviewLayoutType);
          break;
        case nameof(INavigationNoteList.PreviewTileSize):
          AppSettingsService.Save(PreviewTileSizeSettingsCodec.Encode, NavigationSettingsDescriptors.TrashPagePreviewTileSize, node.PreviewTileSize);
          break;
        case nameof(INavigationNoteList.PreviewTileRatio):
          AppSettingsService.Save(PreviewTileRatioSettingsCodec.Encode, NavigationSettingsDescriptors.TrashPagePreviewTileRatio, node.PreviewTileRatio);
          break;
      }
    }
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
              NoteSortKey = node.NoteSortKey
            }
          });
          break;
        case nameof(NavigationUserLeafNode.NoteSortDirection):
          await NavigationService.Modification.UpdateNavigationViewStateAsync(new UpdateNavigationViewStateAppCommand()
          {
            PatchDto = new LeafNavigationViewStatePatchDto()
            {
              Id = node.Id,
              NoteSortDirection = node.NoteSortDirection
            }
          });
          break;
        case nameof(NavigationUserLeafNode.PreviewLayoutType):
          await NavigationService.Modification.UpdateNavigationViewStateAsync(new UpdateNavigationViewStateAppCommand()
          {
            PatchDto = new LeafNavigationViewStatePatchDto()
            {
              Id = node.Id,
              PreviewLayoutType = node.PreviewLayoutType
            }
          });
          break;
        case nameof(NavigationUserLeafNode.PreviewTileSize):
          await NavigationService.Modification.UpdateNavigationViewStateAsync(new UpdateNavigationViewStateAppCommand()
          {
            PatchDto = new LeafNavigationViewStatePatchDto()
            {
              Id = node.Id,
              PreviewTileSize = node.PreviewTileSize
            }
          });
          break;
        case nameof(NavigationUserLeafNode.PreviewTileRatio):
          await NavigationService.Modification.UpdateNavigationViewStateAsync(new UpdateNavigationViewStateAppCommand()
          {
            PatchDto = new LeafNavigationViewStatePatchDto()
            {
              Id = node.Id,
              PreviewTileRatio = node.PreviewTileRatio
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