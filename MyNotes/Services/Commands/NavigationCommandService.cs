using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Application.Contracts.Navigations.Enums;
using MyNotes.Application.Dtos.Navigations.Arrangement;
using MyNotes.Application.Dtos.Navigations.Common;
using MyNotes.Application.Dtos.Navigations.Creation;
using MyNotes.Application.Dtos.Navigations.Modification;
using MyNotes.Application.Enums.Navigations;
using MyNotes.Application.Services.Navigations;
using MyNotes.Common.Commands;
using MyNotes.Common.Enums.Modes;
using MyNotes.Common.Helpers;
using MyNotes.Common.Structures;
using MyNotes.Domain.ValueObjects;
using MyNotes.Mappers;
using MyNotes.Models.Navigations;
using MyNotes.Services.Dialogs;
using MyNotes.Services.Navigations;
using MyNotes.Services.Windows;
using MyNotes.Templates;

namespace MyNotes.Services.Commands;

internal sealed class NavigationCommandService : ICommandService
{
  private readonly NavigationController NavigationController;
  private readonly NavigationService NavigationService;
  private readonly MainWindowService MainWindowService;
  private readonly DialogService DialogService;

  public AsyncCommand<NavigationUserNode> AddListCommand { get; }
  public Command<NavigationUserNode> AddGroupCommand { get; }
  public Command<NavigationUserNode> UpdateCommand { get; }
  public Command<NavigationUserNode> DeleteCommand { get; }
  public Command<SourceTargetPair<NavigationUserNode, NavigationUserNode>> MoveToCommand { get; }
  public Command<SourceTargetPair<NavigationUserNode, NavigationUserCompositeNode>> MoveToGroupCommand { get; }
  public Command<NavigationUserNode> SetAsStartPageCommand { get; }

  public NavigationCommandService(NavigationController navigationController, NavigationService navigationService, MainWindowService mainWindowService, DialogService dialogService)
  {
    NavigationController = navigationController;
    NavigationService = navigationService;
    MainWindowService = mainWindowService;
    DialogService = dialogService;

    AddListCommand = new()
    {
      ExecuteFunc = async (navigation) => await AddNavigationAsync(navigation, false)
    };

    AddGroupCommand = new()
    {
      ExecuteAction = async (navigation) => await AddNavigationAsync(navigation, true)
    };

    UpdateCommand = new()
    {
      ExecuteAction = async (navigation) =>
      {
        if (MainWindowService.TryGetCurrentWindow(out var mainWindow) && mainWindow.Content.XamlRoot is XamlRoot xamlRoot)
        {
          // MainWindow에 Title, Icon 변경할 수 있는 ContentDialog 띄움
          var dialogResponse = await DialogService.ShowEditUserNavigationDialogAsync(xamlRoot, navigation, EditMode.Update, navigation is NavigationUserCompositeNode);

          if (dialogResponse.Result is ContentDialogResult.Primary && dialogResponse.Data is (Icon, string) userInput)
          {
            // Application 계층에 업데이트 요청 및
            // 실제 변경된 필드와 값들을 요청에 대한 응답으로 받아서 뷰 Navigation에 반영
            UpdateNavigationAppRequestDto updateUserNavigationAppRequestDto = new()
            {
              Id = navigation.Id,
              UpdateFields = NavigationUpdateFields.Icon | NavigationUpdateFields.Title,
              Icon = userInput.Icon,
              Title = userInput.Title
            };

            // 요청 및 응답
            UpdateNavigationAppResponseDto updateUserNavigationAppResponseDto = await NavigationService.Modification.UpdateNavigationAsync(updateUserNavigationAppRequestDto);

            // 실제 변경된 필드에 대한 동작
            var changedNavigationFields = updateUserNavigationAppResponseDto.ChangedFields;
            if (changedNavigationFields.HasFlag(NavigationChangedFields.Icon) && updateUserNavigationAppResponseDto.Icon is Icon updatedIcon)
            {
              if (userInput.Icon != updatedIcon)
              {
                //todo: Icon 입력값과 반환값이 다를 때 동작 구현
              }
              navigation.Icon = updatedIcon;
            }
            if (changedNavigationFields.HasFlag(NavigationChangedFields.Title) && updateUserNavigationAppResponseDto.Title is string updatedTitle)
            {
              if (userInput.Title != updatedTitle)
              {
                //todo: Title 입력값과 반환값이 다를 때 동작 구현
              }
              navigation.Title = updatedTitle;
            }
          }
        }
      }
    };

    DeleteCommand = new()
    {
      ExecuteAction = async (navigation) =>
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
            DeleteNavigationAppRequestDto deleteUserNavigationAppRequestDto = new()
            {
              Id = navigation.Id,
              DeleteMode = dialogResponse.Data
            };

            if (await NavigationService.Modification.DeleteNavigationAsync(deleteUserNavigationAppRequestDto))
            {
              navigation.Parent.ChildNodes.Remove(navigation);
            }
          }
        }
      }
    };

    MoveToCommand = new()
    {
      ExecuteAction = async (navigationPair) =>
      {
        var sourceNavigation = navigationPair.Source;
        var targetNavigation = navigationPair.Target;

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

        MoveNavigationAppRequestDto appRequestDto = new()
        {
          SourceNavigation = sourceNavigation.Id,
          TargetNavigation = targetNavigation.Id,
          InsertPosition = insertPosition,
          ExpectedTargetSiblings = expectedTargetSiblings
        };

        MoveNavigationAppResponseDto appResponseDto = await NavigationService.Arrangement.MoveNavigationAsync(appRequestDto);

        if (!appResponseDto.IsMoveApplied)
        {
          return;
        }

        var updatedNavigations = appResponseDto.UpdatedNavigations!.ToList();
        int desiredSourceIndex = updatedNavigations.IndexOf(sourceNavigation.Id);

        if (desiredSourceIndex < 0)
        {
          throw new InvalidOperationException();
        }

        switch (appResponseDto.ResultKind)
        {
          case MoveNavigationResultKind.MovedAsRequested:
            ApplySingleNavigationMove(sourceNavigation, sourceParent, targetParent, desiredSourceIndex);
            break;

          case MoveNavigationResultKind.MovedWithOrderReconciliation:
            ApplySingleNavigationMove(sourceNavigation, sourceParent, targetParent, desiredSourceIndex);
            SynchronizeNavigationOrder(targetParent.ChildNodes, updatedNavigations);
            break;

          case MoveNavigationResultKind.Rejected:
            break;
        }
      }
    };

    MoveToGroupCommand = new()
    {
      ExecuteAction = async (navigationPair) =>
      {
        var sourceNavigation = navigationPair.Source;
        var targetGroupNavigation = navigationPair.Target;

        // 이미 같은 그룹이면 이동 불필요
        if (targetGroupNavigation == sourceNavigation.Parent)
        {
          return;
        }

        // 이동 요청 및 결과 확인
        MoveNavigationAppRequestDto appRequestDto = new()
        {
          SourceNavigation = sourceNavigation.Id,
          TargetNavigation = targetGroupNavigation.Id,
          InsertPosition = NavigationInsertPosition.LastChild,
          ExpectedTargetSiblings = [.. targetGroupNavigation.ChildNodes.Select(n => n.Id)]
        };

        MoveNavigationAppResponseDto appResponseDto = await NavigationService.Arrangement.MoveNavigationAsync(appRequestDto);
        if (appResponseDto.IsMoveApplied)
        {
          // UI 계층에서 이동 사항 반영
          sourceNavigation.Parent.ChildNodes.Remove(sourceNavigation);
          targetGroupNavigation.ChildNodes.Add(sourceNavigation);
        }
        else
        {
          //todo: 이동 실패 시 동작 구현
        }
      }
    };

    SetAsStartPageCommand = new()
    {
      ExecuteAction = (navigation) =>
      {

      },
      CanExecuteFunc = (navigation) => navigation is NavigationUserLeafNode
    };
  }

  private async Task AddNavigationAsync(NavigationUserNode targetNavigation, bool isNavigationComposite, CancellationToken cancellationToken = default)
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

        CreateNavigationAppRequestDto createUserNavigationAppRequestDto = new()
        {
          InsertTargetId = targetNavigation.Id,
          InsertPosition = navigationInsertPosition,
          IsComposite = isNavigationComposite,
          Icon = v.Icon,
          Title = v.Title,
        };

        if (await NavigationService.Creation.AddNavigationAsync(createUserNavigationAppRequestDto, cancellationToken) is NavigationBundleAppResponseDto responseDto)
        {
          NavigationUserCompositeNode parentNavigation = targetNavigation switch
          {
            NavigationUserCompositeNode => targetNavigation as NavigationUserCompositeNode ?? throw new InvalidOperationException(),
            NavigationUserLeafNode => targetNavigation.Parent,
            _ => throw new NotSupportedException($"지원하지 않는 NavigationUserNode 파생 타입: {targetNavigation.GetType().FullName}")
          };

          var newNavigation = NavigationMappers.ToModel(responseDto, parentNavigation);
          
          if(targetNavigation is NavigationUserCompositeNode compositeNode)
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
}
