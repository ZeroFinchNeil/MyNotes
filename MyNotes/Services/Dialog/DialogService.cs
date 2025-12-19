using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models;
using MyNotes.Models.Navigations;
using MyNotes.Services.Navigation;
using MyNotes.Templates;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Dialogs;
using MyNotes.Views.Dialogs;

namespace MyNotes.Services.Dialog;

internal sealed partial class DialogService
{
  private readonly DialogViewModelFactory ViewModelFactory;

  public DialogService(DialogViewModelFactory viewModelFactory)
  {
    ViewModelFactory = viewModelFactory;
  }

  public async Task<(ContentDialogResult ContentDialogResult, (Icon Icon, string Title)? Value)> ShowAddNodeDialogAsync(XamlRoot xamlRoot, NavigationUserNode targetNode, bool isCompositeNode)
  {
    if (ViewModelFactory.Resolve(DialogType.SetNode, targetNode, isCompositeNode) is SetUserNavigationDialogViewModel viewModel)
    {
      var dialog = new AddUserNavigationDialog(viewModel) { XamlRoot = xamlRoot };
      var dialogResult = await dialog.ShowAsync();
      return (dialogResult, viewModel.Result);
    }
    return (ContentDialogResult.None, null);
  }

  public async Task<ContentDialogResult> ShowConfirmDeleteDialogAsync(XamlRoot xamlRoot, string targetTypeName, string targetName, DeleteMode deleteMode)
  {
    if(ViewModelFactory.Resolve(DialogType.ConfirmDelete, targetTypeName, targetName, deleteMode) is ConfirmDeleteDialogViewModel viewModel)
    {
      var dialog = new ConfirmDeleteDialog(viewModel) { XamlRoot = xamlRoot };
      return await dialog.ShowAsync();
    }
    return ContentDialogResult.None;
  }
}