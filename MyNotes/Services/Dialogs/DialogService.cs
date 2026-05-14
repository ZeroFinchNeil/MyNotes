using MyNotes.Common.Enums.Modes;
using MyNotes.Domain.ValueObjects;
using MyNotes.Models.Navigations;
using MyNotes.Models.UI;
using MyNotes.Templates;
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

  public async Task<DialogResponse<(Icon Icon, string Title)?>> ShowEditUserNavigationDialogAsync(XamlRoot xamlRoot, NavigationUserNode targetNavigation, EditMode editMode, bool isCompositeNode)
  {
    if (ViewModelFactory.Create(DialogType.EditUserNavigation, targetNavigation, editMode, isCompositeNode) is EditUserNavigationDialogViewModel viewmodel)
    {
      ContentDialog dialog = editMode switch
      {
        EditMode.Create => new CreateUserNavigationDialog(viewmodel),
        EditMode.Update => new UpdateUserNavigationDialog(viewmodel),
        _ => throw new ArgumentException("Invalid edit mode")
      };
      dialog.XamlRoot = xamlRoot;
      var contentDialogResult = await ShowNewDialog(dialog);

      return new() { Result = contentDialogResult, Data = viewmodel.Result };
    }

    return new() { Result = ContentDialogResult.None, Data = null };
  }

  public async Task<DialogResponse<DeleteMode>> ShowConfirmDeleteDialogAsync(XamlRoot xamlRoot, string targetTypeName, string targetName, DeleteMode deleteMode)
  {
    if (ViewModelFactory.Create(DialogType.ConfirmDelete, targetTypeName, targetName, deleteMode) is ConfirmDeleteDialogViewModel viewmodel)
    {
      var dialog = new ConfirmDeleteDialog(viewmodel) { XamlRoot = xamlRoot };
      return new() { Result = await ShowNewDialog(dialog), Data = viewmodel.DeleteMode };
    }
    return new() { Result = ContentDialogResult.None, Data = DeleteMode.Permanent };
  }

  public async Task<DialogResponse<NavigationId?>> ShowSelectNoteParentDialogAsync(XamlRoot xamlRoot)
  {
    if (ViewModelFactory.Create(DialogType.SelectNoteParent) is SelectNoteParentDialogViewModel viewmodel)
    {
      var dialog = new SelectNoteParentDialog(viewmodel) { XamlRoot = xamlRoot };
      return new() { Result = await ShowNewDialog(dialog), Data = viewmodel.SelectedNavigationViewModel?.Navigation.Id };
    }
    return new() { Result = ContentDialogResult.None, Data = null };
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