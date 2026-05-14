using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Common.Enums.Modes;

namespace MyNotes.ViewModels.Dialogs;

internal sealed partial class ConfirmDeleteDialogViewModel : DialogViewModelBase
{
  [ObservableProperty]
  public partial string TargetCategory { get; set; }

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
  public ConfirmDeleteDialogViewModel(string targetCategory, string targetName, DeleteMode deleteMode)
  {
    TargetCategory = targetCategory;
    TargetName = targetName;
    DeleteMode = deleteMode;

    _deletePermanently = deleteMode is DeleteMode.Permanent;
  }
  #endregion
}
