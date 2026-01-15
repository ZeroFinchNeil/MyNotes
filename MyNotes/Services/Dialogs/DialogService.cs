using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Modes;
using MyNotes.Models.Navigations;
using MyNotes.Services.Navigations;
using MyNotes.Templates;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Dialogs;
using MyNotes.Views.Dialogs;

namespace MyNotes.Services.Dialogs;

internal sealed class DialogService
{
  private readonly DialogViewModelFactory ViewModelFactory;

  public DialogService(DialogViewModelFactory viewmodelFactory)
  {
    ViewModelFactory = viewmodelFactory;
  }

  public async Task<(ContentDialogResult ContentDialogResult, (Icon Icon, string Title)? Value)> ShowEditUserNavigationDialogAsync(XamlRoot xamlRoot, NavigationUserNode targetNavigation, EditMode editMode, bool isCompositeNode)
  {
    if (ViewModelFactory.Resolve(DialogType.SetNode, targetNavigation, editMode, isCompositeNode) is EditUserNavigationDialogViewModel viewmodel)
    {
      ContentDialog dialog = editMode switch
      {
        EditMode.Create => new CreateUserNavigationDialog(viewmodel),
        EditMode.Update => new UpdateUserNavigationDialog(viewmodel),
        _ => throw new ArgumentException("Invalid edit mode")
      };
      dialog.XamlRoot = xamlRoot;
      var dialogResult = await ShowNewDialog(dialog);

      return (dialogResult, viewmodel.Result);
    }
    return (ContentDialogResult.None, null);
  }

  public async Task<ContentDialogResult> ShowConfirmDeleteDialogAsync(XamlRoot xamlRoot, string targetTypeName, string targetName, DeleteMode deleteMode)
  {
    if (ViewModelFactory.Resolve(DialogType.ConfirmDelete, targetTypeName, targetName, deleteMode) is ConfirmDeleteDialogViewModel viewmodel)
    {
      var dialog = new ConfirmDeleteDialog(viewmodel) { XamlRoot = xamlRoot };
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