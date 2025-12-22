using MyNotes.Helpers;
using MyNotes.Resources;
using MyNotes.ViewModels.Dialogs;

namespace MyNotes.Views.Dialogs;

internal sealed partial class CreateUserNavigationDialog : ContentDialog
{
  private readonly EditUserNavigationDialogViewModel ViewModel;

  public CreateUserNavigationDialog(EditUserNavigationDialogViewModel viewModel)
  {
    InitializeComponent();
    ViewModel = viewModel;

    string nodeType = (viewModel.IsCompositeNode ? LocalizedStrings.NavigationUserCompositeNode_DisplayName : LocalizedStrings.NavigationUserLeafNodeDisplayName).ToLower();

    CreateUserNavigationDialog_ContentDialog.Title = string.Format(LocalizedStrings.CreateUserNavigationDialogTitleText, nodeType);

    CreateUserNavigationDialog_SubtitleTextBlock.Text = string.Format(LocalizedStrings.CreateUserNavigationDialogSubTitleTextBlockText, StringHelper.Inflect(nodeType), viewModel.Target.Title);
  }
}
