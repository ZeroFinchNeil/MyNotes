using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Navigations;
using MyNotes.Services.Navigation;
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

  public async Task<(ContentDialogResult ContentDialogResult, NavigationUserNode? AddedNavigation)> ShowAddNodeDialogAsync(XamlRoot xamlRoot, NavigationUserNode targetNode)
  {
    if (ViewModelFactory.Resolve(DialogType.SetNode, targetNode) is SetUserNavigationDialogViewModel viewModel)
    {
      var dialog = new AddUserNavigationDialog(viewModel) { XamlRoot = xamlRoot };
      var dialogResult = await dialog.ShowAsync();
      var addedNavigation = viewModel.Result is Task<NavigationUserNode> t ? await t : null;
      return (dialogResult, addedNavigation);
    }
    return (ContentDialogResult.None, null);
  }
}