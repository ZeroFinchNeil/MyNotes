using MyNotes.Services.Navigations;
using MyNotes.ViewModels.Navigations;

namespace MyNotes.ViewModels.Dialogs;

internal sealed partial class SelectNoteParentDialogViewModel : DialogViewModelBase
{
  public ObservableCollection<UserLeafNavigationViewModel> TargetNavigationViewModels { get; } = new();

  public UserLeafNavigationViewModel? SelectedNavigationViewModel
  {
    get;
    set => SetProperty(ref field, value);
  }

  #region Object Lifetime Management
  public SelectNoteParentDialogViewModel(NavigationService navigationService, NavigationViewModelProvider navigationViewModelProvider)
  {
    foreach (var viewmodel in navigationViewModelProvider.Resolve(navigationService.UserLeafNavigations))
    {
      if (viewmodel is UserLeafNavigationViewModel targetVM)
      {
        TargetNavigationViewModels.Add(targetVM);
      }
    }
  }
  #endregion
}
