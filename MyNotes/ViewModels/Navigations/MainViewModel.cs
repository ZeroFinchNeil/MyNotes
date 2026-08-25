using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Commands;
using MyNotes.Domain.Navigations;
using MyNotes.Models.Navigations;
using MyNotes.Models.Navigations.Core;
using MyNotes.Models.Navigations.User;
using MyNotes.Services.Commands;
using MyNotes.Services.Navigations;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Navigations.Providers;

namespace MyNotes.ViewModels;

internal sealed partial class MainViewModel : ViewModelBase
{
  private readonly NavigationController NavigationController;
  private readonly NavigationViewModelProvider NavigationViewModelProvider;
  private readonly NavigationCommandService NavigationCommandService;

  // Header
  private readonly LeasedNavigationViewModelCollection _headerMenuItemLeases;
  public IReadOnlyList<NavigationViewModelBase> HeaderMenuItems => _headerMenuItemLeases.ViewModels;

  // User
  public IViewModelLease<NavigationViewModelBase> _userRootNavigationViewModelLease;
  public UserRootGroupNavigationViewModel UserRootNavigationViewModel => (UserRootGroupNavigationViewModel)_userRootNavigationViewModelLease.ViewModel;
  //public IReadOnlyList<NavigationViewModelBase> UserNavigationViewModels => UserRootNavigationViewModel.ChildNodeViewModels;

  // Footer
  private readonly LeasedNavigationViewModelCollection _footerMenuItemLeases;
  public IReadOnlyList<NavigationViewModelBase> FooterMenuItems => _footerMenuItemLeases.ViewModels;

  public ReadOnlyObservableCollection<NavigationViewModelBase> MenuItems { get; }

  [ObservableProperty]
  public partial NavigationViewModelBase? CurrentNavigationViewModel { get; set; }

  #region Object Lifetime Management
  public MainViewModel(NavigationController navigationController, NavigationViewModelProvider navigationViewModelProvider, [FromKeyedServices(CommandServiceType.Navigation)] ICommandService navigationCommandService)
  {
    // DI
    NavigationController = navigationController;
    NavigationViewModelProvider = navigationViewModelProvider;
    NavigationCommandService = (NavigationCommandService)navigationCommandService;

    _headerMenuItemLeases = new(NavigationController.PrimaryCoreNavigations.Select(NavigationViewModelProvider.Resolve));
    _userRootNavigationViewModelLease = NavigationViewModelProvider.Resolve(NavigationController.UserRootNavigation);
    _footerMenuItemLeases = new(NavigationController.SecondaryCoreNavigations.Select(NavigationViewModelProvider.Resolve));
    MenuItems = new([.. HeaderMenuItems, UserRootNavigationViewModel]);

    NavigationController.CurrentNavigationChanged += NavigationController_CurrentNavigationChanged;

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
      NavigationController.CurrentNavigationChanged -= NavigationController_CurrentNavigationChanged;
      NavigationController.ResetNavigation();
      _headerMenuItemLeases.Dispose();
      _userRootNavigationViewModelLease.Dispose();
      _footerMenuItemLeases.Dispose();
    }

    base.Dispose(disposing);
  }
  #endregion

  private void NavigationController_CurrentNavigationChanged(object sender, INavigation? args)
  {
    SyncNavigation();
    CanNavigateBack = NavigationController.NavigationBackStack.Count > 0;
  }

  public void NavigateTo(INavigation navigation)
  {
    AddListCommand.NotifyCanExecuteChanged();
    AddGroupCommand.NotifyCanExecuteChanged();
    NavigationController.NavigateTo(navigation);
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

  public void NavigateBack() => NavigationController.NavigateBack();

  public async Task MoveNavigationAsync(NavigationUserNode sourceNavigation, NavigationUserNode targetNavigation)
  {
    await NavigationCommandService.MoveToAsync(sourceNavigation, targetNavigation);
    SyncNavigation();
  }

  public void SyncNavigation()
  {
    if (CurrentNavigationViewModel?.Navigation != NavigationController.CurrentNavigation)
    {
      switch (NavigationController.CurrentNavigation)
      {
        case INavigationNode node:
          using (var lease = NavigationViewModelProvider.Acquire(node))
          {
            if (lease is not null)
            {
              CurrentNavigationViewModel = lease.ViewModel;
            }
          }
          break;
        case NavigationSearch search:
          using (var searchLease = NavigationViewModelProvider.Resolve(search))
          {
            CurrentNavigationViewModel = searchLease.ViewModel;
          }
          break;
        default:
          CurrentNavigationViewModel = null;
          break;
      }
    }
  }

  [ObservableProperty]
  public partial bool CanNavigateBack { get; private set; } = false;

  [ObservableProperty]
  public partial bool IsNavigationPaneOpen { get; private set; } = true;
}

internal sealed partial class MainViewModel : ViewModelBase
{
  public AsyncCommand<NavigationUserNode> AddListCommand { get; private set; }

  public AsyncCommand<NavigationUserNode> AddGroupCommand { get; private set; }

  public Command ToggleNavigationPaneCommand { get; private set; }

  public Command<string> SearchNoteCommand { get; private set; }

  [MemberNotNull(nameof(AddListCommand), nameof(AddGroupCommand), nameof(ToggleNavigationPaneCommand), nameof(SearchNoteCommand))]
  private void SetCommands()
  {
    AddListCommand = new()
    {
      ExecuteFunc = (targetNavigation) => NavigationCommandService.AddNavigationAsync(targetNavigation, false)
    };

    AddGroupCommand = new()
    {
      ExecuteFunc = (targetNavigation) => NavigationCommandService.AddNavigationAsync(targetNavigation, true)
    };

    ToggleNavigationPaneCommand = new()
    {
      ExecuteAction = () => IsNavigationPaneOpen = !IsNavigationPaneOpen
    };

    SearchNoteCommand = new()
    {
      ExecuteAction = async (searchText) =>
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