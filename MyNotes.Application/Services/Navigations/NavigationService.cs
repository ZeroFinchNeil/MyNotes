namespace MyNotes.Application.Services.Navigations;

internal sealed partial class NavigationService
{
  public NavigationTreeService Tree { get; }
  public NavigationCreationService Creation { get; }
  public NavigationRetrievalService Retrieval { get; }
  public NavigationModificationService Modification { get; }
  public NavigationArrangementService Arrangement { get; }

  public NavigationService(NavigationTreeService navigationTreeService, NavigationCreationService navigationCreationService, NavigationRetrievalService navigationRetrievalService, NavigationModificationService navigationModificationService, NavigationArrangementService navigationArrangementService)
  {
    Tree = navigationTreeService;
    Creation = navigationCreationService;
    Retrieval = navigationRetrievalService;
    Modification = navigationModificationService;
    Arrangement = navigationArrangementService;
  }

#if false
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

  public void MoveNavigation(SourceTargetPair<NavigationUserNode, NavigationUserNode> navigationPair)
  {
    var sourceNavigation = navigationPair.Source;
    var targetNavigation = navigationPair.Target;

    if (sourceNavigation == targetNavigation)
    {
      return;
    }

    var sourceParentNavigation = sourceNavigation.Parent;
    var targetParentNavigation = targetNavigation.Parent;
    int targetIndex = targetParentNavigation.ChildNodes.IndexOf(targetNavigation);

    sourceParentNavigation.ChildNodes.Remove(sourceNavigation);
    targetParentNavigation.ChildNodes.Insert(targetIndex, sourceNavigation);
  }

  public void MoveNavigationToGroup(SourceTargetPair<NavigationUserNode, NavigationUserCompositeNode> navigationPair)
  {
    var sourceItem = navigationPair.Source;
    var targetGroup = navigationPair.Target;
    if (targetGroup.CanBeParentOf(sourceItem))
    {
      sourceItem.Parent.ChildNodes.Remove(sourceItem);
      targetGroup.ChildNodes.Add(sourceItem);
    }
  }
#endif
}
