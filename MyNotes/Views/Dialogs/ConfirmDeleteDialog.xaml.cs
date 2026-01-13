using MyNotes.ViewModels.Dialogs;

namespace MyNotes.Views.Dialogs;

internal sealed partial class ConfirmDeleteDialog : ContentDialog
{
  private readonly ConfirmDeleteDialogViewModel ViewModel;

  public ConfirmDeleteDialog(ConfirmDeleteDialogViewModel viewModel)
  {
    InitializeComponent();
    ViewModel = viewModel;

    this.Loaded += ConfirmDeleteDialog_Loaded;
    this.Unloaded += ConfirmDeleteDialog_Unloaded;
  }

  private void ConfirmDeleteDialog_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private void ConfirmDeleteDialog_Unloaded(object sender, RoutedEventArgs e)
  {
    Bindings.StopTracking();
  }
}
