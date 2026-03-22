using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Services.Navigations;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Navigations.Providers;

namespace MyNotes.ViewModels.Dialogs;

internal sealed partial class SelectNoteParentDialogViewModel : DialogViewModelBase
{
  public ObservableCollection<UserLeafNavigationViewModel> TargetNavigationViewModels { get; } = new();

  [ObservableProperty]
  public partial UserLeafNavigationViewModel? SelectedNavigationViewModel { get; set; }

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
