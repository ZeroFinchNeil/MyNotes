using MyNotes.Common.Commands;
using MyNotes.Common.Structures;
using MyNotes.Models.Modes;
using MyNotes.Models.Navigations;
using MyNotes.Services.App;
using MyNotes.Services.Dialogs;
using MyNotes.Services.Navigations;
using MyNotes.Templates;

namespace MyNotes.Services.Commands;

internal sealed class NavigationCommandService : ICommandService
{
  private readonly NavigationService NavigationService;
  private readonly NavigationTreeService NavigationTreeService;
  private readonly WindowService WindowService;
  private readonly DialogService DialogService;

  public Command<NavigationUserNode> AddListCommand { get; }
  public Command<NavigationUserNode> AddGroupCommand { get; }
  public Command<NavigationUserNode> UpdateCommand { get; }
  public Command<NavigationUserNode> DeleteCommand { get; }
  public Command<SourceTargetPair<NavigationUserNode, NavigationUserCompositeNode>> MoveToGroupCommand { get; }
  public Command<NavigationUserNode> SetAsStartPageCommand { get; }

  public NavigationCommandService(NavigationService navigationService, NavigationTreeService navigationTreeService, WindowService windowService, DialogService dialogService)
  {
    NavigationService = navigationService;
    NavigationTreeService = navigationTreeService;
    WindowService = windowService;
    DialogService = dialogService;

    AddListCommand = new()
    {
      ActionToExecute = async (navigation) =>
      {
        if (WindowService.TryGetCurrentMainWindow(out var mainWindow)
            && mainWindow.Content.XamlRoot is XamlRoot xamlRoot)
        {
          var result = await DialogService.ShowEditUserNavigationDialogAsync(xamlRoot, navigation, EditMode.Create, false);
          if (result is { ContentDialogResult: ContentDialogResult.Primary, Value: (Icon, string) v }
              && await NavigationTreeService.AddUserNodeAsync(targetNode: navigation, isCompositeNode: false, icon: v.Icon, title: v.Title) is INavigation newNavigation)
          {
            NavigationService.NavigateTo(newNavigation);
          }
        }
      }
    };

    AddGroupCommand = new()
    {
      ActionToExecute = async (navigation) =>
    {
      if (WindowService.TryGetCurrentMainWindow(out var mainWindow)
          && mainWindow.Content.XamlRoot is XamlRoot xamlRoot)
      {
        var result = await DialogService.ShowEditUserNavigationDialogAsync(xamlRoot, navigation, EditMode.Create, true);
        if (result is { ContentDialogResult: ContentDialogResult.Primary, Value: (Icon, string) v }
            && await NavigationTreeService.AddUserNodeAsync(targetNode: navigation, isCompositeNode: true, icon: v.Icon, title: v.Title) is INavigation newNavigation)
        {
          NavigationService.NavigateTo(newNavigation);
        }
      }
    }
    };

    UpdateCommand = new()
    {
      ActionToExecute = async (navigation) =>
      {
        if (WindowService.TryGetCurrentMainWindow(out var mainWindow)
            && mainWindow.Content.XamlRoot is XamlRoot xamlRoot)
        {
          var result = await DialogService.ShowEditUserNavigationDialogAsync(xamlRoot, navigation, EditMode.Update, navigation is NavigationUserCompositeNode);
          if (result.ContentDialogResult == ContentDialogResult.Primary && result.Value is (Icon, string) v)
          {
            string title = v.Title;

            navigation.Icon = v.Icon;
            navigation.Title = title;
          }
        }
      }
    };

    DeleteCommand = new()
    {
      ActionToExecute = async (navigation) =>
      {
        if (WindowService.TryGetCurrentMainWindow(out var mainWindow)
            && mainWindow.Content.XamlRoot is XamlRoot xamlRoot)
        {
          var targetTypeName = navigation switch
          {
            NavigationUserLeafNode => "List",
            NavigationUserCompositeNode => "Group",
            _ => string.Empty
          };
          var deleteMode = DeleteMode.MoveToTrash;
          var result = await DialogService.ShowConfirmDeleteDialogAsync(xamlRoot, targetTypeName, navigation.Title, deleteMode);
          if (result.ContentDialogResult == ContentDialogResult.Primary)
          {
            await NavigationTreeService.DeleteUserNodeAsync(navigation, result.DeleteMode);
          }
        }
      }
    };

    MoveToGroupCommand = new()
    {
      ActionToExecute = (pair) => NavigationTreeService.MoveNavigationToGroup(pair)
    };

    SetAsStartPageCommand = new()
    {
      ActionToExecute = (navigation) =>
      {

      },
      CanExecuteFunc = (navigation) => navigation is NavigationUserLeafNode
    };
  }
}
