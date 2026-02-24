using MyNotes.Models.Modes;

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

  public DeleteMode DeleteMode
  {
    get;
    set => SetProperty(ref field, value);
  }

  private bool _deletePermanently;
  public bool DeletePermanently
  {
    get => _deletePermanently;
    set
    {
      if (SetProperty(ref _deletePermanently, value))
      {
        DeleteMode = value ? DeleteMode.Permanent : DeleteMode.MoveToTrash;
      }
    }
  }

  #region Object Lifetime Management
  public ConfirmDeleteDialogViewModel(string targetTypeName, string targetName, DeleteMode deleteMode)
  {
    TargetTypeName = targetTypeName;
    TargetName = targetName;
    DeleteMode = deleteMode;

    _deletePermanently = deleteMode is DeleteMode.Permanent;
  }
  #endregion
}
