using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Commands;
using MyNotes.Common.Structures;
using MyNotes.Domain.Navigations;
using MyNotes.Models.Navigations;
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
  public IReadOnlyList<NavigationViewModelBase> HeaderMenuItems { get; }

  // User
  public UserRootGroupNavigationViewModel UserRootNavigationViewModel { get; }
  //public IReadOnlyList<NavigationViewModelBase> UserNavigationViewModels => UserRootNavigationViewModel.ChildNodeViewModels;

  // Footer
  public IReadOnlyList<NavigationViewModelBase> FooterMenuItems { get; }

  private readonly ObservableCollection<NavigationViewModelBase> _menuItems;
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

    HeaderMenuItems = [.. NavigationViewModelProvider.Resolve(NavigationController.PrimaryCoreNavigations)];
    UserRootNavigationViewModel = (UserRootGroupNavigationViewModel)NavigationViewModelProvider.Resolve(NavigationController.UserRootNavigation);
    FooterMenuItems = [.. NavigationViewModelProvider.Resolve(NavigationController.SecondaryCoreNavigations)];
    _menuItems = [.. HeaderMenuItems, UserRootNavigationViewModel];
    //_menuItems = [.. HeaderMenuItems, .. UserNavigationViewModels];
    MenuItems = new(_menuItems);

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
    }

    base.Dispose(disposing);
  }
  #endregion

  private void NavigationController_CurrentNavigationChanged(object sender, INavigation? args)
  {
    //ConsoleHelper.WriteLine(true, "{0}: {1}", "navigation", (args as NavigationUserNode)?.Title);
    //ConsoleHelper.WriteLine(true, "{0}: {1}", "NavigationController.NavigationBackStack.Count", NavigationController.NavigationBackStack.Count);
    //ConsoleHelper.WriteLine(true, "{0}: {1}", "CurrentNavigationViewModel", CurrentNavigationViewModel);
    SyncNavigation();
    CanNavigateBack = NavigationController.NavigationBackStack.Count > 0;
    //ConsoleHelper.WriteLine(true, "{0}: {1}", "CurrentNavigationViewModel", CurrentNavigationViewModel);
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
      CurrentNavigationViewModel = NavigationController.CurrentNavigation switch
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