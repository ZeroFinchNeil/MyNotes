using MyNotes.Common.Commands;
using MyNotes.Models.Modes;
using MyNotes.Models.Navigations;
using MyNotes.Services.Navigations;
using MyNotes.Templates;

namespace MyNotes.ViewModels.Dialogs;

internal sealed partial class EditUserNavigationDialogViewModel : DialogViewModelBase
{
  public NavigationUserNode Target { get; }
  public bool IsCompositeNode { get; }
  public EditMode EditMode { get; }

  public (Icon Icon, string Title)? Result { get; private set; }

  public EditUserNavigationDialogViewModel(NavigationUserNode targetNavigation, EditMode editMode, bool isCompositeNode)
  {
    Target = targetNavigation;
    EditMode = editMode;
    IsCompositeNode = isCompositeNode;

    switch (EditMode)
    {
      case EditMode.Create:
        _icon = isCompositeNode ? Icon.System_Notebook : Icon.System_Board;
        break;
      case EditMode.Update:
        _icon = (Icon)targetNavigation.Icon;
        _title = targetNavigation.Title;
        break;
    }

    SetCommands();
  }

  private Icon _icon = Icon.System_Board;
  public Icon Icon
  {
    get => _icon;
    set => SetProperty(ref _icon, value);
  }

  private string _title = string.Empty;
  public string Title
  {
    get => _title;
    set => SetProperty(ref _title, value);
  }
}

internal sealed partial class EditUserNavigationDialogViewModel : DialogViewModelBase
{
  public Command? PrimaryButtonCommand { get; private set; }

  private void SetCommands()
  {
    PrimaryButtonCommand = new(
      actionToExecute: () => Result = (Icon, Title)
    );
  }
}