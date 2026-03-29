using MyNotes.Resources;
using MyNotes.ViewModels.Dialogs;

namespace MyNotes.Views.Dialogs;

[Debugging.ReferenceTracker]
internal sealed partial class UpdateUserNavigationDialog : ContentDialog
{
  private readonly EditUserNavigationDialogViewModel ViewModel;

  public UpdateUserNavigationDialog(EditUserNavigationDialogViewModel viewmodel)
  {
    TrackReference();
    InitializeComponent();
    ViewModel = viewmodel;

    string nodeType = (viewmodel.IsCompositeNode ? LocalizedStrings.NavigationUserCompositeNodeDisplayName : LocalizedStrings.NavigationUserLeafNodeDisplayName).ToLower();

    UpdateUserNavigationDialog_ContentDialog.Title = string.Format(LocalizedStrings.UpdateUserNavigationDialogTitleText, nodeType);

    UpdateUserNavigationDialog_SubtitleTextBlock.Text = string.Format(LocalizedStrings.UpdateUserNavigationDialogSubTitleTextBlockText, viewmodel.Target.Title);

    this.Loaded += UpdateUserNavigationDialog_Loaded;
    this.Unloaded += UpdateUserNavigationDialog_Unloaded;
  }

  private void UpdateUserNavigationDialog_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private void UpdateUserNavigationDialog_Unloaded(object sender, RoutedEventArgs e)
  {
    Bindings.StopTracking();
  }
}
