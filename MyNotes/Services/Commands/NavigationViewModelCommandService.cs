using MyNotes.Common.Commands;
using MyNotes.Common.Structures;
using MyNotes.Models.Modes;
using MyNotes.Models.Navigations;
using MyNotes.Services.Dialogs;
using MyNotes.Services.Navigations;
using MyNotes.Services.Windows;
using MyNotes.Templates;
using MyNotes.ViewModels.Navigations;

namespace MyNotes.Services.Commands;

internal sealed class NavigationViewModelCommandService : ICommandService
{
  private readonly NavigationService NavigationService;
  private readonly WindowService WindowService;
  private readonly DialogService DialogService;

  public Command<NavigationViewModelBase> AddListCommand { get; }
  public Command<NavigationViewModelBase> AddGroupCommand { get; }
  public Command<NavigationViewModelBase> UpdateCommand { get; }
  public Command<NavigationViewModelBase> DeleteCommand { get; }
  public Command<SourceTargetPair<NavigationViewModelBase, NavigationViewModelBase>> MoveToGroupCommand { get; }
  public Command<NavigationViewModelBase> SetAsStartPageCommand { get; }

  public NavigationViewModelCommandService(NavigationService navigationService, WindowService windowService, DialogService dialogService)
  {
    NavigationService = navigationService;
    WindowService = windowService;
    DialogService = dialogService;

    AddListCommand = new()
    {
      ActionToExecute = async (targetViewModel) =>
      {
        if (targetViewModel.Navigation is NavigationUserNode navigation
            && WindowService.TryGetCurrentMainWindow(out var mainWindow)
            && mainWindow.Content.XamlRoot is XamlRoot xamlRoot)
        {
          var result = await DialogService.ShowEditUserNavigationDialogAsync(xamlRoot, navigation, EditMode.Create, false);
          if (result is { ContentDialogResult: ContentDialogResult.Primary, Value: (Icon, string) v }
              && await NavigationService.AddUserNodeAsync(targetNode: navigation, isCompositeNode: false, icon: v.Icon, title: v.Title) is INavigation newNavigation)
          {
            NavigationService.PushNavigation(newNavigation);
          }
        }
      }
    };

    AddGroupCommand = new()
    {
      ActionToExecute = async (targetViewModel) =>
    {
      if (targetViewModel.Navigation is NavigationUserNode navigation
          && WindowService.TryGetCurrentMainWindow(out var mainWindow)
          && mainWindow.Content.XamlRoot is XamlRoot xamlRoot)
      {
        var result = await DialogService.ShowEditUserNavigationDialogAsync(xamlRoot, navigation, EditMode.Create, true);
        if (result is { ContentDialogResult: ContentDialogResult.Primary, Value: (Icon, string) v }
            && await NavigationService.AddUserNodeAsync(targetNode: navigation, isCompositeNode: true, icon: v.Icon, title: v.Title) is INavigation newNavigation)
        {
          NavigationService.PushNavigation(newNavigation);
        }
      }
    }
    };

    UpdateCommand = new()
    {
      ActionToExecute = async (targetViewModel) =>
      {
        if (targetViewModel.Navigation is NavigationUserNode navigation
            && WindowService.TryGetCurrentMainWindow(out var mainWindow)
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
      ActionToExecute = async (targetViewModel) =>
      {
        if (targetViewModel.Navigation is NavigationUserNode navigation
            && WindowService.TryGetCurrentMainWindow(out var mainWindow)
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
            await NavigationService.DeleteUserNodeAsync(navigation, result.DeleteMode);
          }
        }
      }
    };

    MoveToGroupCommand = new()
    {
      ActionToExecute = (pair) =>
      {
        if (pair.Source.Navigation is NavigationUserNode sourceItem
        && pair.Target.Navigation is NavigationUserCompositeNode targetGroup)
        {
          if (targetGroup.CanBeParentOf(sourceItem))
          {
            sourceItem.Parent.ChildNodes.Remove(sourceItem);
            targetGroup.ChildNodes.Add(sourceItem);
          }
        }
      }
    };

    SetAsStartPageCommand = new()
    {
      ActionToExecute = (viewmodel) =>
      {

      },
      CanExecuteFunc = (viewmodel) =>
      {
        return viewmodel is UserLeafNavigationViewModel;
      }
    };
  }
}
