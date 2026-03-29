using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Common.Commands;
using MyNotes.Models.Modes;
using MyNotes.Models.Navigations;
using MyNotes.Templates;

namespace MyNotes.ViewModels.Dialogs;

internal sealed partial class EditUserNavigationDialogViewModel : DialogViewModelBase
{
  public NavigationUserNode Target { get; }
  public bool IsCompositeNode { get; }
  public EditMode EditMode { get; }

  public (Icon Icon, string Title)? Result { get; private set; }

  #region Object Lifetime Management
  public EditUserNavigationDialogViewModel(NavigationUserNode targetNavigation, EditMode editMode, bool isCompositeNode)
  {
    Target = targetNavigation;
    EditMode = editMode;
    IsCompositeNode = isCompositeNode;

    switch (EditMode)
    {
      case EditMode.Create:
        Icon = isCompositeNode ? Icon.System_Notebook : Icon.System_Board;
        break;
      case EditMode.Update:
        Icon = targetNavigation.Icon;
        Title = targetNavigation.Title;
        break;
    }

    SetCommands();
  }
  #endregion

  [ObservableProperty]
  public partial Icon Icon { get; set; } = Icon.System_Board;

  [ObservableProperty]
  public partial string Title { get; set; } = string.Empty;
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