using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Services.Navigations;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Navigations.Providers;

namespace MyNotes.ViewModels.Dialogs;

internal sealed partial class SelectNoteParentDialogViewModel : DialogViewModelBase
{
  public ObservableCollection<UserListNavigationViewModel> TargetNavigationViewModels { get; } = new();

  [ObservableProperty]
  public partial UserListNavigationViewModel? SelectedNavigationViewModel { get; set; }

  #region Object Lifetime Management
  public SelectNoteParentDialogViewModel(NavigationController navigationController, NavigationViewModelProvider navigationViewModelProvider)
  {
    foreach(var navigation in navigationController.UserLeafNavigations)
    {
      using var lease = navigationViewModelProvider.Resolve(navigation);
      if (lease.ViewModel is UserListNavigationViewModel targetVM)
      {
        TargetNavigationViewModels.Add(targetVM);
      }
    }
  }
  #endregion
}
