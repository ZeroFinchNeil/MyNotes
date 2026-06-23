using MyNotes.Shared.Constants;
using MyNotes.ViewModels.Dialogs;

namespace MyNotes.Views.Dialogs;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class ConfirmDeleteDialog : ContentDialog
{
  private readonly ConfirmDeleteDialogViewModel ViewModel;

  #region #region Object Lifetime Management
  public ConfirmDeleteDialog(ConfirmDeleteDialogViewModel viewmodel)
  {
    TrackReference();
    InitializeComponent();
    ViewModel = viewmodel;

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
  #endregion

  private string SetTitle(string title) => $"{LocalizedStrings.ConfirmDeleteDialogAffixTitleTextBlockText} {title}";
}
