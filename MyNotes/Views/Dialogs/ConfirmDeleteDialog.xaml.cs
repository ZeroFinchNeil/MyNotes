using MyNotes.AppConstants;
using MyNotes.Debugging;
using MyNotes.Resources;
using MyNotes.ViewModels.Dialogs;

namespace MyNotes.Views.Dialogs;

internal sealed partial class ConfirmDeleteDialog : ContentDialog
{
  private readonly ConfirmDeleteDialogViewModel ViewModel;

  #region #region Object Lifetime Management
  public ConfirmDeleteDialog(ConfirmDeleteDialogViewModel viewmodel)
  {
#if DEBUG
    if (Debugger.IsAttached)
    {
      ReferenceTracker.ElementReference.Add(this, $"{GetType().Name}: {GetHashCode()}");
    }
#endif
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
