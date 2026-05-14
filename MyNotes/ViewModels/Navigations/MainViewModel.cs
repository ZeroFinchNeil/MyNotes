using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Application.Dtos.Navigations;
using MyNotes.Application.Services.Navigations;
using MyNotes.Common.Commands;
using MyNotes.Common.Structures;
using MyNotes.Domain.ValueObjects;
using MyNotes.Models.Navigations;
using MyNotes.Services.Commands;
using MyNotes.Services.Navigations;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Navigations.Providers;

namespace MyNotes.ViewModels;

internal sealed partial class MainViewModel : ViewModelBase
{
  private readonly NavigationController NavigationController;
  private readonly NavigationService NavigationService;
  private readonly NavigationViewModelProvider NavigationViewModelProvider;
  private readonly NavigationCommandService NavigationCommandService;

  // Header
  public IReadOnlyList<NavigationViewModelBase> HeaderMenuItems { get; }

  // User
  public UserRootGroupNavigationViewModel UserRootNavigationViewModel { get; }
  public IReadOnlyList<NavigationViewModelBase> UserNavigationViewModels => UserRootNavigationViewModel.ChildNodeViewModels;

  // Footer
  public IReadOnlyList<NavigationViewModelBase> FooterMenuItems { get; }

  private readonly ObservableCollection<NavigationViewModelBase> _menuItems;
  public ReadOnlyObservableCollection<NavigationViewModelBase> MenuItems { get; }

  [ObservableProperty]
  public partial NavigationViewModelBase? CurrentNavigationViewModel { get; set; }

  #region Object Lifetime Management
  public MainViewModel(NavigationController navigationController, NavigationService navigationService, NavigationViewModelProvider navigationViewModelProvider, [FromKeyedServices(CommandServiceType.Navigation)] ICommandService navigationCommandService)
  {
    // DI
    NavigationController = navigationController;
    NavigationService = navigationService;
    NavigationViewModelProvider = navigationViewModelProvider;
    NavigationCommandService = (NavigationCommandService)navigationCommandService;

    HeaderMenuItems = [.. NavigationViewModelProvider.Resolve(NavigationController.PrimaryCoreNavigations)];
    UserRootNavigationViewModel = (UserRootGroupNavigationViewModel)NavigationViewModelProvider.Resolve(NavigationController.UserRootNavigation);
    FooterMenuItems = [.. NavigationViewModelProvider.Resolve(NavigationController.SecondaryCoreNavigations)];
    _menuItems = [.. HeaderMenuItems, UserRootNavigationViewModel];
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
    Console.WriteLine("{0}: {1}", "navigation", (args as NavigationUserNode)?.Title);
    Console.WriteLine("{0}: {1}", "NavigationController.NavigationBackStack.Count", NavigationController.NavigationBackStack.Count);
    Console.WriteLine("{0}: {1}", "CurrentNavigationViewModel", CurrentNavigationViewModel);
    SyncNavigation();
    CanNavigateBack = NavigationController.NavigationBackStack.Count > 0;
    Console.WriteLine("{0}: {1}", "CurrentNavigationViewModel", CurrentNavigationViewModel);
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

  public async Task MoveNavigationAsync(SourceTargetPair<NavigationUserNode, NavigationUserNode> navigationPair)
  {
    var sourceNavigation = navigationPair.Source;
    var targetNavigation = navigationPair.Target;

    NavigationInsertPosition insertPosition;
    var targetParent = targetNavigation.Parent;
    var expectedTargetSiblings = targetParent.ChildNodes.Select(n => n.Id).ToList();
    var targetIndex = targetParent.ChildNodes.IndexOf(targetNavigation);

    if (sourceNavigation.Parent == targetParent)
    {
      var sourceIndex = targetParent.ChildNodes.IndexOf(sourceNavigation);
      if (sourceIndex < 0 || targetIndex < 0)
      {
        //todo: 상세 예외로 교체
        throw new InvalidOperationException();
      }

      if (sourceIndex == targetIndex)
      {
        return;
      }

      insertPosition = sourceIndex < targetIndex ? NavigationInsertPosition.After : NavigationInsertPosition.Before;

      expectedTargetSiblings.RemoveAt(sourceIndex);
    }
    else
    {
      insertPosition = NavigationInsertPosition.Before;
    }

    expectedTargetSiblings.Insert(targetIndex, sourceNavigation.Id);

    MoveUserNavigationAppRequestDto appRequestDto = new()
    {
      SourceNavigation = sourceNavigation.Id,
      TargetNavigation = targetNavigation.Id,
      NavigationInsertPosition = insertPosition,
      ExpectedTargetSiblings = expectedTargetSiblings
    };

    await NavigationService.Arrangement.MoveUserNavigationAsync(appRequestDto);
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