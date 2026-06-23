using MyNotes.ViewModels.Dialogs;

namespace MyNotes.Views.Dialogs;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class SelectNoteParentDialog : ContentDialog
{
  private readonly SelectNoteParentDialogViewModel ViewModel;

  public SelectNoteParentDialog(SelectNoteParentDialogViewModel viewmodel)
  {
    TrackReference();
    InitializeComponent();

    ViewModel = viewmodel;

    this.Loaded += SelectNoteParentDialog_Loaded;
    this.Unloaded += SelectNoteParentDialog_Unloaded;
  }

  private void SelectNoteParentDialog_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private void SelectNoteParentDialog_Unloaded(object sender, RoutedEventArgs e)
  {
    Bindings.StopTracking();
  }

  private bool IsNotNull(object? obj) => obj is not null;
}
