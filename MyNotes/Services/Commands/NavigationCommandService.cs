using DotNext;

using MyNotes.Application.Commands.Navigations;
using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Application.Navigations.Commands;
using MyNotes.Application.Navigations.Results;
using MyNotes.Application.Navigations.Services;
using MyNotes.Application.Results;
using MyNotes.Common.Commands;
using MyNotes.Common.Enums.Modes;
using MyNotes.Common.Helpers;
using MyNotes.Common.Mappers;
using MyNotes.Common.Structures;
using MyNotes.Domain.Navigations;
using MyNotes.Models.Navigations;
using MyNotes.Services.Dialogs;
using MyNotes.Services.Navigations;
using MyNotes.Services.Windows;
using MyNotes.Templates;

namespace MyNotes.Services.Commands;

internal sealed class NavigationCommandService : ICommandService
{
  private readonly NavigationService NavigationService;
  private readonly MainWindowService MainWindowService;
  private readonly DialogService DialogService;

  public NavigationCommandService(NavigationService navigationService, MainWindowService mainWindowService, DialogService dialogService)
  {
    NavigationService = navigationService;
    MainWindowService = mainWindowService;
    DialogService = dialogService;
  }

  public async Task AddNavigationAsync(NavigationUserNode targetNavigation, bool isNavigationComposite, CancellationToken cancellationToken = default)
  {
    if (MainWindowService.TryGetCurrentWindow(out var mainWindow) && mainWindow.Content.XamlRoot is XamlRoot xamlRoot)
    {
      var dialogResponse = await DialogService.ShowEditUserNavigationDialogAsync(xamlRoot, targetNavigation, EditMode.Create, false);
      if (dialogResponse is { Result: ContentDialogResult.Primary, Data: (Icon, string) v })
      {
        NavigationInsertPosition navigationInsertPosition = targetNavigation switch
        {
          NavigationUserCompositeNode => NavigationInsertPosition.LastChild,
          NavigationUserLeafNode => NavigationInsertPosition.After,
          _ => throw new NotSupportedException($"지원하지 않는 NavigationUserNode 파생 타입: {targetNavigation.GetType().FullName}")
        };

        CreateNavigationAppCommand appCommand = new()
        {
          IsComposite = isNavigationComposite,
          Icon = (int)v.Icon,
          Title = v.Title,
          InsertTargetId = targetNavigation.Id,
          InsertPosition = navigationInsertPosition
        };

        if (await NavigationService.Creation.AddNavigationAsync(appCommand, cancellationToken) is NavigationDto navigationDto)
        {
          NavigationUserCompositeNode parentNavigation = targetNavigation switch
          {
            NavigationUserCompositeNode => targetNavigation as NavigationUserCompositeNode ?? throw new InvalidOperationException(),
            NavigationUserLeafNode => targetNavigation.Parent,
            _ => throw new NotSupportedException($"지원하지 않는 NavigationUserNode 파생 타입: {targetNavigation.GetType().FullName}")
          };

          var newNavigation = NavigationMappers.ToModel(navigationDto, parentNavigation);

          if (targetNavigation is NavigationUserCompositeNode compositeNode)
          {
            compositeNode.ChildNodes.Add(newNavigation);
          }
          else if (targetNavigation is NavigationUserLeafNode)
          {

            parentNavigation.ChildNodes.Insert(parentNavigation.ChildNodes.IndexOf(targetNavigation) + 1, newNavigation);
          }
          //NavigationUserNode navigation = new()
          //NavigationController.NavigateTo(navigation);
        }
        else
        {
          throw new Exception();
        }
      }
    }
  }

  public async Task ChangeNavigationTitleAndIconAsync(NavigationUserNode navigation)
  {
    if (MainWindowService.TryGetCurrentWindow(out var mainWindow) && mainWindow.Content.XamlRoot is XamlRoot xamlRoot)
    {
      // MainWindow에 Title, Icon 변경할 수 있는 ContentDialog 띄움
      var dialogResponse = await DialogService.ShowEditUserNavigationDialogAsync(xamlRoot, navigation, EditMode.Update, navigation is NavigationUserCompositeNode);

      if (dialogResponse.Result is ContentDialogResult.Primary && dialogResponse.Data is (Icon, string) userInput)
      {
        // 요청 및 응답
        NavigationPatchDto patchDto = new()
        {
          Id = navigation.Id,
          Icon = navigation.Icon != userInput.Icon ? (int)userInput.Icon : Optional<int>.None,
          Title = navigation.Title != userInput.Title ? userInput.Title : Optional<string>.None,
        };
        UpdateNavigationAppCommand appCommand = new() { PatchDto = patchDto };
        var updateResult = await NavigationService.Modification.UpdateNavigationAsync(appCommand);

        if (updateResult is AppUpdateStatus.Succeeded)
        {
          if (patchDto.Icon.TryGet(out var updatedIcon) && Enum.IsDefined(typeof(Icon), updatedIcon))
          {
            navigation.Icon = (Icon)updatedIcon;
          }

          if (patchDto.Title.TryGet(out var updatedTitle))
          {
            navigation.Title = updatedTitle;
          }
        }
      }
    }
  }

  public async Task DeleteNavigationAsync(NavigationUserNode navigation)
  {
    if (MainWindowService.TryGetCurrentWindow(out var mainWindow) && mainWindow.Content.XamlRoot is XamlRoot xamlRoot)
    {
      // MainWindow에 Navigation 삭제 ContentDialog 띄움
      var targetCategory = navigation switch
      {
        NavigationUserLeafNode => "List",
        NavigationUserCompositeNode => "Group",
        _ => string.Empty
      };

      //todo: 앱 Settings에서 휴지통에 넣을 것인지 완전히 삭제할 것인지 결정
      var deleteMode = DeleteMode.MoveToTrash;

      var dialogResponse = await DialogService.ShowConfirmDeleteDialogAsync(xamlRoot, targetCategory, navigation.Title, deleteMode);
      if (dialogResponse.Result is ContentDialogResult.Primary)
      {
        DeleteNavigationAppCommand appCommand = new()
        {
          Id = navigation.Id,
          DeleteMode = dialogResponse.Data
        };

        if (await NavigationService.Modification.DeleteNavigationAsync(appCommand) is AppUpdateStatus.Succeeded)
        {
          navigation.Parent.ChildNodes.Remove(navigation);
        }
      }
    }
  }

  public async Task MoveToGroupAsync(NavigationUserNode sourceNavigation, NavigationUserCompositeNode targetGroupNavigation)
  {
    // 이미 같은 그룹이면 이동 불필요
    if (targetGroupNavigation == sourceNavigation.Parent)
    {
      return;
    }

    // 이동 요청 및 결과 확인
    MoveNavigationAppCommand appCommand = new()
    {
      SourceNavigationId = sourceNavigation.Id,
      TargetNavigationId = targetGroupNavigation.Id,
      InsertPosition = NavigationInsertPosition.LastChild,
      ExpectedTargetSiblings = [.. targetGroupNavigation.ChildNodes.Select(n => n.Id)]
    };

    var moveResult = await NavigationService.Arrangement.MoveNavigationAsync(appCommand);

    switch (moveResult.Kind)
    {
      case MoveNavigationResultKind.MovedAsRequested:
        sourceNavigation.Parent.ChildNodes.Remove(sourceNavigation);
        targetGroupNavigation.ChildNodes.Add(sourceNavigation);
        break;
      //todo: 이동 실패 및 재정렬 요구 시 동작 구현
      case MoveNavigationResultKind.MovedWithOrderReconciliation:
        break;
      case MoveNavigationResultKind.Rejected:
        break;
    }
  }

  public async Task SetAsStartPageAsync(NavigationUserNode navigation)
  {
    throw new NotImplementedException();
  }

  public async Task MoveToAsync(NavigationUserNode sourceNavigation, NavigationUserNode targetNavigation)
  {
    NavigationInsertPosition insertPosition;
    var sourceParent = sourceNavigation.Parent;
    var targetParent = targetNavigation.Parent;
    var expectedTargetSiblings = targetParent.ChildNodes.Select(n => n.Id).ToList();
    var targetIndex = targetParent.ChildNodes.IndexOf(targetNavigation);
    var sourceIndex = targetParent.ChildNodes.IndexOf(sourceNavigation);

    if (targetIndex < 0)
    {
      throw new InvalidOperationException();
    }

    if (sourceParent == targetParent)
    {
      if (sourceIndex < 0 || targetIndex < 0)
      {
        //todo: 상세 예외로 교체
        throw new InvalidOperationException();
      }

      if (sourceIndex == targetIndex)
      {
        return;
      }

      insertPosition = sourceIndex < targetIndex ? NavigationInsertPosition.After : NavigationInsertPosition.Before;

      expectedTargetSiblings.RemoveAt(sourceIndex);
    }
    else
    {
      insertPosition = NavigationInsertPosition.Before;
    }

    expectedTargetSiblings.Insert(targetIndex, sourceNavigation.Id);

    MoveNavigationAppCommand appCommand = new()
    {
      SourceNavigationId = sourceNavigation.Id,
      TargetNavigationId = targetNavigation.Id,
      InsertPosition = insertPosition,
      ExpectedTargetSiblings = expectedTargetSiblings
    };

    var moveResult = await NavigationService.Arrangement.MoveNavigationAsync(appCommand);

    if (moveResult.Kind is MoveNavigationResultKind.Rejected)
    {
      return;
    }

    var updatedNavigations = moveResult.UpdatedNavigations!.ToList();
    int desiredSourceIndex = updatedNavigations.IndexOf(sourceNavigation.Id);

    if (desiredSourceIndex < 0)
    {
      throw new InvalidOperationException();
    }

    switch (moveResult.Kind)
    {
      case MoveNavigationResultKind.MovedAsRequested:
        ApplySingleNavigationMove(sourceNavigation, sourceParent, targetParent, desiredSourceIndex);
        break;
      case MoveNavigationResultKind.MovedWithOrderReconciliation:
        ApplySingleNavigationMove(sourceNavigation, sourceParent, targetParent, desiredSourceIndex);
        SynchronizeNavigationOrder(targetParent.ChildNodes, updatedNavigations);
        break;
    }
  }

  private static void ApplySingleNavigationMove(NavigationUserNode sourceNavigation, NavigationUserCompositeNode sourceParent, NavigationUserCompositeNode targetParent, int desiredSourceIndex)
  {
    if (sourceParent != targetParent)
    {
      sourceParent.ChildNodes.Remove(sourceNavigation);
      sourceNavigation.Parent = targetParent;
      targetParent.ChildNodes.Insert(desiredSourceIndex, sourceNavigation);
      return;
    }

    int currentSourceIndex = targetParent.ChildNodes.IndexOf(sourceNavigation);

    if (currentSourceIndex < 0)
    {
      throw new InvalidOperationException();
    }

    if (currentSourceIndex != desiredSourceIndex)
    {
      //targetParent.ChildNodes.Move(currentSourceIndex, desiredSourceIndex);
      targetParent.ChildNodes.RemoveAt(currentSourceIndex);
      targetParent.ChildNodes.Insert(desiredSourceIndex, sourceNavigation);
    }
  }

  private static void SynchronizeNavigationOrder(ObservableCollection<NavigationUserNode> currentNavigations, IReadOnlyList<NavigationId> updatedNavigationIds)
  {
    int count = currentNavigations.Count;

    // Application 응답은 같은 Navigation 집합을 보장하지만,
    // 현재 Presentation 컬렉션이 응답 시점까지 동일한 상태인지 확인함
    if (count != updatedNavigationIds.Count)
    {
      throw new InvalidOperationException();
    }

    if (count <= 1)
    {
      return;
    }

    // 기존 컬렉션의 Navigation을 Id 기준으로 찾기 위한 매핑입니다.
    // 동시에 각 Navigation의 현재(기존) 인덱스도 저장함
    Dictionary<NavigationId, (NavigationUserNode Navigation, int Index)> currentStateById = currentNavigations
      .Select((navigation, index) => (navigation, index))
      .ToDictionary(item => item.navigation.Id, item => (Navigation: item.navigation, Index: item.index));

    // updatedNavigationIds는 목표 순서입니다.
    // 각 목표 항목이 현재 컬렉션에서는 몇 번째에 있는지 숫자 배열로 변환함
    // 예:
    // 순서: 0 1 2 3 4
    // 기존: A B C D E
    // 목표: B D A C E
    // currentIndexesInTargetOrder:
    // B D A C E
    // 1 3 0 2 4
    int[] currentIndexesInTargetOrder = new int[count];

    for (int targetIndex = 0; targetIndex < count; targetIndex++)
    {
      NavigationId navigationId = updatedNavigationIds[targetIndex];

      if (!currentStateById.TryGetValue(navigationId, out var currentState))
      {
        throw new InvalidOperationException();
      }

      currentIndexesInTargetOrder[targetIndex] = currentState.Index;
    }

    // LIS에 포함되는 원소들의 인덱스의 값이 true가 되는 컬렉션 생성
    var fixedTargetIndexes = CollectionHelper.FindLISIndexFlags(currentIndexesInTargetOrder);

    // LIS에 포함되지 않은 항목만 Move하므로 CollectionChanged 이벤트 발생을 최소화함
    for (int targetIndex = 0; targetIndex < count; targetIndex++)
    {
      NavigationId expectedNavigationId = updatedNavigationIds[targetIndex];

      if (fixedTargetIndexes[targetIndex])
      {
        if (currentNavigations[targetIndex].Id != expectedNavigationId)
        {
          throw new InvalidOperationException();
        }

        continue;
      }

      NavigationUserNode targetNavigation = currentStateById[expectedNavigationId].Navigation;
      int currentIndex = currentNavigations.IndexOf(targetNavigation);

      if (currentIndex < 0)
      {
        throw new InvalidOperationException();
      }

      if (currentIndex != targetIndex)
      {
        //currentNavigations.Move(currentIndex, targetIndex);
        currentNavigations.RemoveAt(currentIndex);
        currentNavigations.Insert(targetIndex, targetNavigation);
      }
    }
  }

  public async Task ViewNavigationListPageAsync(NavigationId navigationId)
  {
    var mainWindow = await MainWindowService.GetOrCreate(navigationId);
    mainWindow.Activate();
  }
}
