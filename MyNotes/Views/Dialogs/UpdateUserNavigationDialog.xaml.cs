using MyNotes.Resources;
using MyNotes.ViewModels.Dialogs;

namespace MyNotes.Views.Dialogs;

internal sealed partial class UpdateUserNavigationDialog : ContentDialog
{
  private readonly EditUserNavigationDialogViewModel ViewModel;

  public UpdateUserNavigationDialog(EditUserNavigationDialogViewModel viewModel)
  {
    InitializeComponent();
    ViewModel = viewModel;

    string nodeType = (viewModel.IsCompositeNode ? LocalizedStrings.NavigationUserCompositeNode_DisplayName : LocalizedStrings.NavigationUserLeafNodeDisplayName).ToLower();

    UpdateUserNavigationDialog_ContentDialog.Title = string.Format(LocalizedStrings.UpdateUserNavigationDialogTitleText, nodeType);

    UpdateUserNavigationDialog_SubtitleTextBlock.Text = string.Format(LocalizedStrings.UpdateUserNavigationDialogSubTitleTextBlockText, viewModel.Target.Title);
  }
}
