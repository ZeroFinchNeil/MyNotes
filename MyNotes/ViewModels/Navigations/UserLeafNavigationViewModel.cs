using MyNotes.Common.Commands;
using MyNotes.Models.Navigations;
using MyNotes.Services.Dialog;
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

  public Command<NavigationViewModelBase>? ShowAddNavigationDialogCommand => NavigationService.ShowAddNavigationDialogCommand;
}
