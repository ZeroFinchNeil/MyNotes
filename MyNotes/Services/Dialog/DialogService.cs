using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Modes;
using MyNotes.Models.Navigations;
using MyNotes.Services.Navigation;
using MyNotes.Templates;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Dialogs;
using MyNotes.Views.Dialogs;

namespace MyNotes.Services.Dialog;

internal sealed class DialogService
{
  private readonly DialogViewModelFactory ViewModelFactory;

  public DialogService(DialogViewModelFactory viewModelFactory)
  {
    ViewModelFactory = viewModelFactory;
  }

  public async Task<(ContentDialogResult ContentDialogResult, (Icon Icon, string Title)? Value)> ShowEditUserNavigationDialogAsync(XamlRoot xamlRoot, NavigationUserNode targetNavigation, EditMode editMode, bool isCompositeNode)
  {
    if (ViewModelFactory.Resolve(DialogType.SetNode, targetNavigation, editMode, isCompositeNode) is EditUserNavigationDialogViewModel viewModel)
    {
      ContentDialog dialog = editMode switch
      {
        EditMode.Create => new CreateUserNavigationDialog(viewModel),
        EditMode.Update => new UpdateUserNavigationDialog(viewModel),
        _ => throw new ArgumentException("Invalid edit mode")
      };
      dialog.XamlRoot = xamlRoot;
      var dialogResult = await ShowNewDialog(dialog);

      return (dialogResult, viewModel.Result);
    }
    return (ContentDialogResult.None, null);
  }

  public async Task<ContentDialogResult> ShowConfirmDeleteDialogAsync(XamlRoot xamlRoot, string targetTypeName, string targetName, DeleteMode deleteMode)
  {
    if (ViewModelFactory.Resolve(DialogType.ConfirmDelete, targetTypeName, targetName, deleteMode) is ConfirmDeleteDialogViewModel viewModel)
    {
      var dialog = new ConfirmDeleteDialog(viewModel) { XamlRoot = xamlRoot };
      return await ShowNewDialog(dialog);
    }
    return ContentDialogResult.None;
  }

  private ContentDialog? _currentDialog;

  private async Task<ContentDialogResult> ShowNewDialog(ContentDialog dialog)
  {
    if (_currentDialog is ContentDialog previousDialog)
    {
      previousDialog.Hide();
    }

    _currentDialog = dialog;
    var result = await dialog.ShowAsync();
    _currentDialog = null;

    return result;
  }
}