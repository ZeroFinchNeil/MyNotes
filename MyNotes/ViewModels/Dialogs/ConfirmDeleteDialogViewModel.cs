using MyNotes.Models;

namespace MyNotes.ViewModels.Dialogs;

internal sealed partial class ConfirmDeleteDialogViewModel : DialogViewModelBase
{
  public string TargetTypeName
  {
    get;
    set => SetProperty(ref field, value);
  }

  public string TargetName
  {
    get;
    set => SetProperty(ref field, value);
  }

  public DeleteMode DeleteMode { get; }

  public ConfirmDeleteDialogViewModel(string targetTypeName, string targetName, DeleteMode deleteMode)
  {
    TargetTypeName = targetTypeName;
    TargetName = targetName;
    DeleteMode = deleteMode;
  }
}
