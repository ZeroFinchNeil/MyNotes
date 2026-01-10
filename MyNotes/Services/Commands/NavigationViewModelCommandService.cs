using MyNotes.Common.Commands;
using MyNotes.Models.Navigations;
using MyNotes.Services.Navigations;
using MyNotes.ViewModels.Navigations;

namespace MyNotes.Services.Commands;

internal sealed class NavigationViewModelCommandService : ICommandService
{
  private readonly NavigationService NavigationService;

  public Command<NavigationViewModelBase> AddListCommand { get; }
  public Command<NavigationViewModelBase> AddGroupCommand { get; }
  public Command<NavigationViewModelBase> UpdateCommand { get; }
  public Command<NavigationViewModelBase> DeleteCommand { get; }
  public Command<(NavigationViewModelBase SourceItemViewModel, NavigationViewModelBase TargetGroupViewModel)> MoveToGroupCommand { get; }

  public NavigationViewModelCommandService(NavigationService navigationService)
  {
    NavigationService = navigationService;

    AddListCommand = new(
      actionToExecute: (targetViewModel) =>
      {
        if (targetViewModel.Navigation is NavigationUserNode navigation)
          NavigationService.AddListCommand?.ActionToExecute?.Invoke(navigation);
      });

    AddGroupCommand = new(
      actionToExecute: (targetViewModel) =>
      {
        if (targetViewModel.Navigation is NavigationUserNode navigation)
          NavigationService.AddGroupCommand?.ActionToExecute?.Invoke(navigation);
      });

    UpdateCommand = new(
      actionToExecute: (targetViewModel) =>
      {
        if (targetViewModel.Navigation is NavigationUserNode navigation)
          NavigationService.UpdateCommand?.ActionToExecute?.Invoke(navigation);
      });

    DeleteCommand = new(
      actionToExecute: (targetViewModel) =>
      {
        if (targetViewModel.Navigation is NavigationUserNode navigation)
          NavigationService.DeleteCommand?.ActionToExecute?.Invoke(navigation);
      });

    MoveToGroupCommand = new(
      actionToExecute: (parameter) =>
      {
        if (parameter.SourceItemViewModel.Navigation is NavigationUserNode sourceItem
        && parameter.TargetGroupViewModel.Navigation is NavigationUserCompositeNode targetGroup)
        {
          NavigationService.MoveToGroupCommand?.ActionToExecute?.Invoke((sourceItem, targetGroup));
        }
      });
  }
}
