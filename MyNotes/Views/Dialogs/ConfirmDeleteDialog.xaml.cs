using MyNotes.ViewModels.Dialogs;

namespace MyNotes.Views.Dialogs;

internal sealed partial class ConfirmDeleteDialog : ContentDialog
{
  private readonly ConfirmDeleteDialogViewModel ViewModel;

  public ConfirmDeleteDialog(ConfirmDeleteDialogViewModel viewModel)
  {
    InitializeComponent();
    ViewModel = viewModel;
  }
}
