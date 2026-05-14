using MyNotes.Application.Contracts.Database.Enums.Navigations;
using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Application.Dtos.Navigations;
using MyNotes.Application.Services.Navigations;
using MyNotes.Common.Commands;
using MyNotes.Common.Enums.Modes;
using MyNotes.Common.Structures;
using MyNotes.Domain.ValueObjects;
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

  public Command<NavigationUserNode> AddListCommand { get; }
  public Command<NavigationUserNode> AddGroupCommand { get; }
  public Command<NavigationUserNode> UpdateCommand { get; }
  public Command<NavigationUserNode> DeleteCommand { get; }
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
      ActionToExecute = async (navigation) => await AddNavigationAsync(navigation, false)
    };

    AddGroupCommand = new()
    {
      ActionToExecute = async (navigation) => await AddNavigationAsync(navigation, true)
    };

    UpdateCommand = new()
    {
      ActionToExecute = async (navigation) =>
      {
        if (MainWindowService.TryGetCurrentWindow(out var mainWindow) && mainWindow.Content.XamlRoot is XamlRoot xamlRoot)
        {
          // MainWindow에 Title, Icon 변경할 수 있는 ContentDialog 띄움
          var dialogResponse = await DialogService.ShowEditUserNavigationDialogAsync(xamlRoot, navigation, EditMode.Update, navigation is NavigationUserCompositeNode);

          if (dialogResponse.Result is ContentDialogResult.Primary && dialogResponse.Data is (Icon, string) userInput)
          {
            // Application 계층에 업데이트 요청 및
            // 실제 변경된 필드와 값들을 요청에 대한 응답으로 받아서 뷰 Navigation에 반영
            UpdateUserNavigationAppRequestDto updateUserNavigationAppRequestDto = new()
            {
              Id = navigation.Id,
              NavigationUpdateField = UserNavigationUpdateFields.Icon | UserNavigationUpdateFields.Title,
              Icon = userInput.Icon,
              Title = userInput.Title
            };

            // 요청 및 응답
            UpdateUserNavigationAppResponseDto updateUserNavigationAppResponseDto = await NavigationService.Modification.UpdateUserNavigationAsync(updateUserNavigationAppRequestDto);

            // 실제 변경된 필드에 대한 동작
            var changedNavigationFields = updateUserNavigationAppResponseDto.ChangedNavigationFields;
            if (changedNavigationFields.HasFlag(UserNavigationChangedFields.Icon) && updateUserNavigationAppResponseDto.Icon is Icon updatedIcon)
            {
              if (userInput.Icon != updatedIcon)
              {
                //todo: Icon 입력값과 반환값이 다를 때 동작 구현
              }
              navigation.Icon = updatedIcon;
            }
            if (changedNavigationFields.HasFlag(UserNavigationChangedFields.Title) && updateUserNavigationAppResponseDto.Title is string updatedTitle)
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
      ActionToExecute = async (navigation) =>
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
            DeleteUserNavigationAppRequestDto deleteUserNavigationAppRequestDto = new()
            {
              Id = navigation.Id,
              DeleteMode = dialogResponse.Data
            };

            if (await NavigationService.Modification.DeleteUserNavigationAsync(deleteUserNavigationAppRequestDto))
            {
              navigation.Parent.ChildNodes.Remove(navigation);
            }
          }
        }
      }
    };

    MoveToGroupCommand = new()
    {
      ActionToExecute = async (navigationPair) =>
      {
        var sourceNavigation = navigationPair.Source;
        var targetGroupNavigation = navigationPair.Target;

        // 이미 같은 그룹이면 이동 불필요
        if (targetGroupNavigation == sourceNavigation.Parent)
        {
          return;
        }

        // 이동 요청 및 결과 확인
        MoveUserNavigationAppRequestDto appRequestDto = new()
        {
          SourceNavigation = sourceNavigation.Id,
          TargetNavigation = targetGroupNavigation.Id,
          NavigationInsertPosition = NavigationInsertPosition.LastChild,
          ExpectedTargetSiblings = [.. targetGroupNavigation.ChildNodes.Select(n => n.Id)]
        };

        MoveUserNavigationAppResponseDto appResponseDto = await NavigationService.Arrangement.MoveUserNavigationAsync(appRequestDto);
        if (appResponseDto.IsMoveAllowed)
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
      ActionToExecute = (navigation) =>
      {

      },
      CanExecuteFunc = (navigation) => navigation is NavigationUserLeafNode
    };
  }

  private async Task AddNavigationAsync(NavigationUserNode navigation, bool isNavigationComposite)
  {
    if (MainWindowService.TryGetCurrentWindow(out var mainWindow) && mainWindow.Content.XamlRoot is XamlRoot xamlRoot)
    {
      var dialogResponse = await DialogService.ShowEditUserNavigationDialogAsync(xamlRoot, navigation, EditMode.Create, false);
      if (dialogResponse is { Result: ContentDialogResult.Primary, Data: (Icon, string) v })
      {
        (NavigationInsertPosition navigationInsertPosition, NavigationId parentId) = navigation switch
        {
          NavigationUserCompositeNode => (NavigationInsertPosition.LastChild, navigation.Id),
          NavigationUserLeafNode => (NavigationInsertPosition.After, navigation.Parent.Id),
          _ => throw new NotSupportedException($"지원하지 않는 NavigationUserNode 파생 타입: {navigation.GetType().FullName}")
        };

        CreateUserNavigationAppRequestDto createUserNavigationAppRequestDto = new()
        {
          InsertTargetId = navigation.Id,
          NavigationInsertPosition = navigationInsertPosition,
          ParentId = parentId,
          IsComposite = isNavigationComposite,
          Icon = v.Icon,
          Title = v.Title,
        };

        if (await NavigationService.Creation.AddUserNavigationAsync(createUserNavigationAppRequestDto) is INavigation newNavigation)
        {
          NavigationController.NavigateTo(newNavigation);
        }
      }
    }
  }
}
