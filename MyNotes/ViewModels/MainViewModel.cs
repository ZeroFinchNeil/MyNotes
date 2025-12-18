using MyNotes.Common.Commands;
using MyNotes.Models.Navigations;
using MyNotes.Services.Navigation;
using MyNotes.ViewModels.Navigations;

namespace MyNotes.ViewModels;

internal sealed partial class MainViewModel : ViewModelBase
{
  private readonly NavigationService NavigationService;
  private readonly NavigationViewModelFactory NavigationViewModelFactory;

  public CollectionViewSource MenuItems { get; private set; } = new() { IsSourceGrouped = true };
  public IReadOnlyList<NavigationViewModelBase>? HeaderMenuItems { get; }
  public UserCompositeNavigationViewModel? UserRootNavigationViewModel { get; }
  public IReadOnlyList<NavigationViewModelBase>? UserNavigationViewModels => UserRootNavigationViewModel?.ChildNodeViewModels;
  public IReadOnlyList<NavigationViewModelBase>? FooterMenuItems { get; }
  //public IReadOnlyList<INavigation> FooterMenuItems2 => NavigationService.SecondaryCoreNavigations;
  //public NavigationUserRootNode UserRootNavigation2 => NavigationService.UserRootNavigation;
  //public IReadOnlyList<INavigationUserNode> UserNavigations2 => NavigationService.UserRootNavigation.ChildNodes;

  public NavigationViewModelBase? CurrentNavigation
  {
    get;
    set => SetProperty(ref field, value);
  }

  public MainViewModel(NavigationService navigationService, NavigationViewModelFactory navigationViewModelFactory)
  {
    NavigationService = navigationService;
    NavigationViewModelFactory = navigationViewModelFactory;

    HeaderMenuItems = [.. NavigationService.PrimaryCoreNavigations.Select(n => NavigationViewModelFactory.Resolve(n))];
    UserRootNavigationViewModel = NavigationViewModelFactory.Resolve(NavigationService.UserRootNavigation) as UserCompositeNavigationViewModel;
    FooterMenuItems = [.. NavigationService.SecondaryCoreNavigations.Select(n => NavigationViewModelFactory.Resolve(n))];
    IReadOnlyList<IReadOnlyList<NavigationViewModelBase>?> MenuItemsSource = [HeaderMenuItems, UserNavigationViewModels];
    MenuItems.Source = MenuItemsSource;

    CurrentNavigation = HeaderMenuItems[0];

    SetCommands();
  }

  protected override void Dispose(bool disposing)
  {
    if (_disposed)
      return;

    if (disposing)
    {

    }

    _disposed = true;
  }
}

internal sealed partial class MainViewModel : ViewModelBase
{
  public Command<NavigationViewModelBase>? AddListCommand => NavigationService.AddListCommand;
  public Command<NavigationViewModelBase>? AddGroupCommand => NavigationService.AddGroupCommand;
  public Command<NavigationUserNode>? ExitUserNodeEditCommand { get; private set; }

  private void SetCommands()
  {
    ExitUserNodeEditCommand = new(node => node.IsEditable = false);
  }
}