using MyNotes.Common.Commands;
using MyNotes.Models.Navigations;
using MyNotes.Services.Navigation;

namespace MyNotes.ViewModels.Navigations;

internal sealed partial class UserLeafNavigationViewModel : NavigationViewModelBase
{
  public override NavigationUserLeafNode Navigation { get; }

  private readonly NavigationService NavigationService;

  public UserLeafNavigationViewModel(NavigationService navigationService, NavigationUserLeafNode navigation)
  {
    Navigation = navigation;

    // Dependency Injection
    NavigationService = navigationService;
  }

  public Command<NavigationViewModelBase>? ShowAddListDialogCommand => NavigationService.ShowAddListDialogCommand;
  public Command<NavigationViewModelBase>? ShowAddGroupDialogCommand => NavigationService.ShowAddGroupDialogCommand;
  public Command<NavigationViewModelBase>? ShowConfirmDeleteDialogCommand => NavigationService.ShowConfirmDeleteDialogCommand;
}
