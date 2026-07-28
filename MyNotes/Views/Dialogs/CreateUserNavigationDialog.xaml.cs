using MyNotes.Common.Helpers;
using MyNotes.Strings;
using MyNotes.ViewModels.Dialogs;

namespace MyNotes.Views.Dialogs;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class CreateUserNavigationDialog : ContentDialog
{
  private readonly EditUserNavigationDialogViewModel ViewModel;

  public CreateUserNavigationDialog(EditUserNavigationDialogViewModel viewmodel)
  {
    TrackReference();
    InitializeComponent();
    ViewModel = viewmodel;

    string nodeType = (viewmodel.IsCompositeNode ? LocalizedStrings.NavigationUserCompositeNodeDisplayName : LocalizedStrings.NavigationUserLeafNodeDisplayName).ToLower();

    CreateUserNavigationDialog_ContentDialog.Title = string.Format(LocalizedStrings.CreateUserNavigationDialogTitleText, nodeType);

    CreateUserNavigationDialog_SubtitleTextBlock.Text = string.Format(LocalizedStrings.CreateUserNavigationDialogSubTitleTextBlockText, StringHelper.Inflect(nodeType), viewmodel.Target.Title);

    this.Loaded += CreateUserNavigationDialog_Loaded;
    this.Unloaded += CreateUserNavigationDialog_Unloaded;
  }

  private void CreateUserNavigationDialog_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private void CreateUserNavigationDialog_Unloaded(object sender, RoutedEventArgs e)
  {
    Bindings.StopTracking();
  }
}
