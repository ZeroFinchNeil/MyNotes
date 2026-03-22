using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Models.Modes;

namespace MyNotes.ViewModels.Dialogs;

internal sealed partial class ConfirmDeleteDialogViewModel : DialogViewModelBase
{
  [ObservableProperty]
  public partial string TargetTypeName { get; set; }

  [ObservableProperty]
  public partial string TargetName { get; set; }

  [ObservableProperty]
  public partial DeleteMode DeleteMode { get; set; }

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
