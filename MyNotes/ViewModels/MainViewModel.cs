using MyNotes.Common.Commands;
using MyNotes.Models.Navigations;
using MyNotes.Services.Commands;
using MyNotes.Services.Navigation;
using MyNotes.ViewModels.Navigations;

namespace MyNotes.ViewModels;

internal sealed partial class MainViewModel : ViewModelBase
{
  private readonly NavigationService NavigationService;
  private readonly NavigationViewModelProvider NavigationViewModelProvider;
  private readonly NavigationCommandService NavigationCommandService;

  public CollectionViewSource MenuItems { get; private set; } = new() { IsSourceGrouped = true };
  public IReadOnlyList<NavigationViewModelBase>? HeaderMenuItems { get; }
  public UserCompositeNavigationViewModel UserRootNavigationViewModel { get; }
  public IReadOnlyList<NavigationViewModelBase>? UserNavigationViewModels => UserRootNavigationViewModel?.ChildNodeViewModels;
  public IReadOnlyList<NavigationViewModelBase>? FooterMenuItems { get; }
  public IReadOnlyList<UserCompositeNavigationViewModel> GroupNavigationViewModels => FindAllGroupNavigationViewModel();

  public NavigationViewModelBase? CurrentNavigationViewModel
  {
    get;
    set
    {
      SetProperty(ref field, value);
    }
  }

  private List<UserCompositeNavigationViewModel> FindAllGroupNavigationViewModel()
  {
    List<UserCompositeNavigationViewModel> viewmodels = new();
    Queue<UserCompositeNavigationViewModel> queue = new();
    queue.Enqueue(UserRootNavigationViewModel);
    while (queue.Count > 0)
    {
      var viewmodel = queue.Dequeue();
      viewmodels.Add(viewmodel);
      foreach (var childViewModel in viewmodel.ChildNodeViewModels)
      {
        if (childViewModel is UserCompositeNavigationViewModel compositeViewModel)
          queue.Enqueue(compositeViewModel);
      }
    }
    return viewmodels;
  }

  public MainViewModel(NavigationService navigationService, NavigationViewModelProvider navigationViewModelProvider, CommandServiceFactory commandServiceFactory)
  {
    NavigationService = navigationService;
    NavigationViewModelProvider = navigationViewModelProvider;
    NavigationCommandService = (NavigationCommandService)commandServiceFactory.Resolve(CommandServiceType.Navigation);

    HeaderMenuItems = [.. NavigationService.PrimaryCoreNavigations.Select(n => NavigationViewModelProvider.Resolve(n))];
    UserRootNavigationViewModel = (UserCompositeNavigationViewModel)NavigationViewModelProvider.Resolve(NavigationService.UserRootNavigation);
    FooterMenuItems = [.. NavigationService.SecondaryCoreNavigations.Select(n => NavigationViewModelProvider.Resolve(n))];
    IReadOnlyList<IReadOnlyList<NavigationViewModelBase>?> MenuItemsSource = [HeaderMenuItems, UserNavigationViewModels];
    MenuItems.Source = MenuItemsSource;

    CurrentNavigationViewModel = HeaderMenuItems[0];

    NavigationService.CurrentNavigationChanged += NavigationService_CurrentNavigationChanged;
    SetCommands();
  }

  private void NavigationService_CurrentNavigationChanged(object sender, INavigation args)
  {
    if (NavigationViewModelProvider.TryResolve(args, out var viewmodel))
      CurrentNavigationViewModel = viewmodel;
  }

  public void PushNavigationBackStack(INavigation navigation)
  {
    NavigationService.PushNavigationBackStack(navigation);
  }

  public void PopNavigationBackStack()
  {
    NavigationService.PopNavigationBackStack();
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
  public Command<NavigationViewModelBase>? AddListCommand => NavigationCommandService.AddListCommand;
  public Command<NavigationViewModelBase>? AddGroupCommand => NavigationCommandService.AddGroupCommand;
  public Command<NavigationUserNode>? ExitUserNodeEditCommand { get; private set; }

  private void SetCommands()
  {
    ExitUserNodeEditCommand = new(node => node.IsEditable = false);
  }
}