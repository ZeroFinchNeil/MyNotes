using MyNotes.Common.Commands;
using MyNotes.Models.Navigations;
using MyNotes.Services.Navigation;
using MyNotes.Templates;

namespace MyNotes.ViewModels.Dialogs;

internal sealed partial class SetUserNavigationDialogViewModel : DialogViewModelBase
{
  public NavigationUserNode Target { get; }
  public bool IsCompositeNode { get; }

  public (Icon Icon, string Title)? Result { get; private set; }

  public SetUserNavigationDialogViewModel(NavigationUserNode targetNavigation, bool isCompositeNode)
  {
    Target = targetNavigation;
    IsCompositeNode = isCompositeNode;
    _icon = isCompositeNode ? Icon.System_Notebook : Icon.System_Board;

    SetCommands();
  }

  private Icon _icon;
  public Icon Icon
  {
    get => _icon;
    set => SetProperty(ref _icon, value);
  }

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
        Result = (Icon, Title);
      }
    );
  }
}