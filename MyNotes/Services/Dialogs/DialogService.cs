using MyNotes.Common.Enums.Modes;
using MyNotes.Domain.Navigations;
using MyNotes.Models.Navigations.User;
using MyNotes.Models.UI;
using MyNotes.Templates;
using MyNotes.ViewModels.Dialogs;
using MyNotes.ViewModels.Dialogs.Providers;
using MyNotes.Views.Dialogs;

namespace MyNotes.Services.Dialogs;

internal sealed class DialogService
{
  private readonly DialogViewModelProvider ViewModelFactory;

  public DialogService(DialogViewModelProvider viewmodelFactory)
  {
    ViewModelFactory = viewmodelFactory;
  }

  public async Task<DialogResponse<(Icon Icon, string Title)?>> ShowEditUserNavigationDialogAsync(XamlRoot xamlRoot, NavigationUserNode targetNavigation, EditMode editMode, bool isCompositeNode)
  {
    using var lease = ViewModelFactory.Resolve(DialogType.EditUserNavigation, targetNavigation, editMode, isCompositeNode);
    if (lease.ViewModel is EditUserNavigationDialogViewModel viewmodel)
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
    using var lease = ViewModelFactory.Resolve(DialogType.ConfirmDelete, targetTypeName, targetName, deleteMode);
    if (lease.ViewModel is ConfirmDeleteDialogViewModel viewmodel)
    {
      var dialog = new ConfirmDeleteDialog(viewmodel) { XamlRoot = xamlRoot };
      return new() { Result = await ShowNewDialog(dialog), Data = viewmodel.DeleteMode };
    }
    return new() { Result = ContentDialogResult.None, Data = DeleteMode.Permanent };
  }

  public async Task<DialogResponse<NavigationId?>> ShowSelectNoteParentDialogAsync(XamlRoot xamlRoot)
  {
    using var lease = ViewModelFactory.Resolve(DialogType.SelectNoteParent);
    if (lease.ViewModel is SelectNoteParentDialogViewModel viewmodel)
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