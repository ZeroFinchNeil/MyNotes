using MyNotes.Common.Commands;
using MyNotes.Models.Navigations;
using MyNotes.Services.Navigation;
using MyNotes.ViewModels.Navigations;

namespace MyNotes.ViewModels;

internal sealed partial class MainViewModel : ViewModelBase
{
  public NavigationService NavigationService { get; }
  private readonly NavigationViewModelFactory NavigationViewModelFactory;

  public CollectionViewSource MenuItems { get; private set; } = new() { IsSourceGrouped = true };
  public IReadOnlyList<NavigationViewModelBase>? HeaderMenuItems { get; }
  public UserCompositeNavigationViewModel? UserRootNavigationViewModel { get; }
  public IReadOnlyList<NavigationViewModelBase>? UserNavigationViewModels => UserRootNavigationViewModel?.ChildNodeViewModels;
  public IReadOnlyList<NavigationViewModelBase>? FooterMenuItems { get; }

  public NavigationViewModelBase? CurrentNavigationViewModel
  {
    get;
    set
    {
      SetProperty(ref field, value);
      Console.WriteLine("{0}: {1}", "Navigation VM Changed", (CurrentNavigationViewModel?.Navigation as INavigationNode)?.Title);
    }
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

    CurrentNavigationViewModel = HeaderMenuItems[0];

    NavigationService.CurrentNavigationChanged += NavigationService_CurrentNavigationChanged;
    SetCommands();
  }

  private void NavigationService_CurrentNavigationChanged(object sender, INavigation args)
  {
    if (NavigationViewModelFactory.ResolvedViewModels.TryGetValue(args, out var wr)
      && wr.TryGetTarget(out var vm))
      CurrentNavigationViewModel = vm;
  }

  protected override void Dispose(bool disposing)
  {
    if (_disposed)
      return;

    if (disposing)
    {
      NavigationService.CurrentNavigationChanged -= NavigationService_CurrentNavigationChanged;
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