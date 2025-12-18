using MyNotes.ViewModels.Dialogs;

namespace MyNotes.Views.Dialogs;

internal sealed partial class AddUserNavigationDialog : ContentDialog
{
  private readonly SetUserNavigationDialogViewModel ViewModel;

  public AddUserNavigationDialog(SetUserNavigationDialogViewModel viewModel)
  {
    InitializeComponent();
    ViewModel = viewModel;
  }
}
