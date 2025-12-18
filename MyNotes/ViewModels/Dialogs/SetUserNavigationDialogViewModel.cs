using MyNotes.Common.Commands;
using MyNotes.Models.Navigations;
using MyNotes.Services.Navigation;
using MyNotes.Templates;

namespace MyNotes.ViewModels.Dialogs;

internal sealed partial class SetUserNavigationDialogViewModel : DialogViewModelBase
{
  private readonly NavigationService NavigationService;

  public NavigationUserNode Target { get; }

  public Task<NavigationUserNode>? Result { get; private set; }

  public SetUserNavigationDialogViewModel(NavigationService navigationService, NavigationUserNode targetNavigation)
  {
    Target = targetNavigation;

    NavigationService = navigationService;
    SetCommands();
  }

  public Icon? Icon
  {
    get;
    set => SetProperty(ref field, value);
  } = Templates.Icon.System_Board;

  public string Title
  {
    get;
    set =>
        SetProperty(ref field, value);
  } = "";
}

internal sealed partial class SetUserNavigationDialogViewModel : DialogViewModelBase
{
  public Command? AddNodeCommand { get; private set; }
  public Command? DeleteNodeCommand { get; private set; }
  public Command? UpdateNodeCommand { get; private set; }

  private void SetCommands()
  {
    AddNodeCommand = new(
      actionToExecute: async () =>
      {
        Result = NavigationService.AddUserNode(navigation: Target, isCompositeNode: Target is NavigationUserCompositeNode, iconName: Icon, title: Title);
      }
    );
  }
}

//internal class OperationContext<TRequest, TResponse>
//{
//  public required TRequest Request { get; init; }
//  public Func<Task<TResponse>>? Operation { get; private set; }
//  public TaskCompletionSource<TResponse> TaskCompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
//  public Task<TResponse> Response => TaskCompletionSource.Task;
//  public OperationState State { get; private set; } = OperationState.Empty;

//  public void SetOperation(Func<Task<TResponse>> operation)
//  {
//    Operation = operation;
//    TaskCompletionSource.Task
//  }
//}

//internal enum OperationState
//{
//  Empty,
//  Ready,
//  Running,
//  Completed
//}