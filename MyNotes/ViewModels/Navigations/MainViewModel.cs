using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Commands;
using MyNotes.Common.Structures;
using MyNotes.Models.Navigations;
using MyNotes.Services.Commands;
using MyNotes.Services.Navigations;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Navigations.Providers;

namespace MyNotes.ViewModels;

internal sealed partial class MainViewModel : ViewModelBase
{
  private readonly NavigationService NavigationService;
  private readonly NavigationTreeService NavigationTreeService;
  private readonly NavigationViewModelProvider NavigationViewModelProvider;
  private readonly NavigationCommandService NavigationCommandService;

  // Header
  public IReadOnlyList<NavigationViewModelBase> HeaderMenuItems { get; }

  // User
  public UserRootNavigationViewModel UserRootNavigationViewModel { get; }
  public IReadOnlyList<NavigationViewModelBase> UserNavigationViewModels => UserRootNavigationViewModel.ChildNodeViewModels;

  // Footer
  public IReadOnlyList<NavigationViewModelBase> FooterMenuItems { get; }

  private readonly ObservableCollection<NavigationViewModelBase> _menuItems;
  public ReadOnlyObservableCollection<NavigationViewModelBase> MenuItems { get; }

  [ObservableProperty]
  public partial NavigationViewModelBase? CurrentNavigationViewModel { get; set; }

  #region Object Lifetime Management
  public MainViewModel(NavigationService navigationService, NavigationTreeService navigationTreeService, NavigationViewModelProvider navigationViewModelProvider, [FromKeyedServices(CommandServiceType.Navigation)] ICommandService navigationCommandService)
  {
    // DI
    NavigationService = navigationService;
    NavigationTreeService = navigationTreeService;
    NavigationViewModelProvider = navigationViewModelProvider;
    NavigationCommandService = (NavigationCommandService)navigationCommandService;

    HeaderMenuItems = [.. NavigationViewModelProvider.Resolve(NavigationService.PrimaryCoreNavigations)];
    UserRootNavigationViewModel = (UserRootNavigationViewModel)NavigationViewModelProvider.Resolve(NavigationService.UserRootNavigation);
    FooterMenuItems = [.. NavigationViewModelProvider.Resolve(NavigationService.SecondaryCoreNavigations)];
    _menuItems = [.. HeaderMenuItems, UserRootNavigationViewModel];
    MenuItems = new(_menuItems);

    NavigationService.CurrentNavigationChanged += NavigationService_CurrentNavigationChanged;

    SetCommands();
  }

  protected override void Dispose(bool disposing)
  {
    if (Disposed)
    {
      return;
    }

    if (disposing)
    {
      NavigationService.CurrentNavigationChanged -= NavigationService_CurrentNavigationChanged;
      NavigationService.ResetNavigation();
    }

    base.Dispose(disposing);
  }
  #endregion

  private void NavigationService_CurrentNavigationChanged(object sender, INavigation? args)
  {
    Console.WriteLine("{0}: {1}", "navigation", (args as NavigationUserNode)?.Title);
    Console.WriteLine("{0}: {1}", "NavigationService.NavigationBackStack.Count", NavigationService.NavigationBackStack.Count);
    Console.WriteLine("{0}: {1}", "CurrentNavigationViewModel", CurrentNavigationViewModel);
    SyncNavigation();
    CanNavigateBack = NavigationService.NavigationBackStack.Count > 0;
    Console.WriteLine("{0}: {1}", "CurrentNavigationViewModel", CurrentNavigationViewModel);
  }

  public void NavigateTo(INavigation navigation)
  {
    AddListCommand.NotifyCanExecuteChanged();
    AddGroupCommand.NotifyCanExecuteChanged();
    NavigationService.NavigateTo(navigation);
  }

  public bool NavigateTo(NavigationId navigationId)
  {
    NavigationViewModelBase? viewmodel =
      HeaderMenuItems.FirstOrDefault(vm => vm.Navigation is NavigationCoreNode coreNode && coreNode.Id == navigationId)
      ?? FooterMenuItems.FirstOrDefault(vm => vm.Navigation is NavigationCoreNode coreNode && coreNode.Id == navigationId)
      ?? UserRootNavigationViewModel.FirstDescendantOrDefault(vm => vm.Navigation is NavigationUserNode userNode && userNode.Id == navigationId, false)
      ?? (HeaderMenuItems.Count > 0 ? HeaderMenuItems[0] : null);

    if (viewmodel is not null)
    {
      NavigateTo(viewmodel.Navigation);
      return true;
    }

    return false;
  }

  public void NavigateBack() => NavigationService.NavigateBack();

  public void MoveNavigation(SourceTargetPair<NavigationUserNode, NavigationUserNode> navigationPair)
  {
    NavigationTreeService.MoveNavigation(navigationPair);
    SyncNavigation();
  }

  public void SyncNavigation()
  {
    if (CurrentNavigationViewModel?.Navigation != NavigationService.CurrentNavigation)
    {
      CurrentNavigationViewModel = NavigationService.CurrentNavigation switch
      {
        INavigationNode node when NavigationViewModelProvider.TryResolve(node, out var viewmodel) => viewmodel,
        NavigationSearch search => NavigationViewModelProvider.Resolve(search),
        _ => null
      };
    }
  }

  [ObservableProperty]
  public partial bool CanNavigateBack { get; private set; } = false;

  [ObservableProperty]
  public partial bool IsNavigationPaneOpen { get; private set; } = true;
}

internal sealed partial class MainViewModel : ViewModelBase
{
  public Command<NavigationUserNode> AddListCommand => NavigationCommandService.AddListCommand;
  public Command<NavigationUserNode> AddGroupCommand => NavigationCommandService.AddGroupCommand;

  public Command? ToggleNavigationPaneCommand { get; private set; }

  public Command<string>? SearchNoteCommand { get; private set; }

  private void SetCommands()
  {
    ToggleNavigationPaneCommand = new()
    {
      ActionToExecute = () => IsNavigationPaneOpen = !IsNavigationPaneOpen
    };

    SearchNoteCommand = new()
    {
      ActionToExecute = async (searchText) =>
      {
        if (string.IsNullOrEmpty(searchText))
        {
          return;
        }

        NavigationSearch navigationSearch = new()
        {
          SearchText = searchText,
          Title = $"Search Results for {searchText}"
        };

        NavigateTo(navigationSearch);
      }
    };
  }
}