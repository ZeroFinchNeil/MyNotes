using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Commands;
using MyNotes.Models.Navigations;
using MyNotes.Services.Commands;
using MyNotes.Services.Navigations;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Navigations.Providers;

namespace MyNotes.ViewModels;

internal sealed partial class MainViewModel : ViewModelBase
{
  private readonly NavigationService NavigationService;
  private readonly NavigationViewModelProvider NavigationViewModelProvider;
  private readonly NavigationCommandService NavigationCommandService;

  // Header
  public IReadOnlyList<NavigationViewModelBase> HeaderMenuItems { get; }

  // User
  public UserRootNavigationViewModel UserRootNavigationViewModel { get; }
  public IReadOnlyList<NavigationViewModelBase> UserNavigationViewModels => UserRootNavigationViewModel.ChildNodeViewModels;

  // Footer
  public IReadOnlyList<NavigationViewModelBase> FooterMenuItems { get; }

  // For CollectionViewSource.Source
  //public IEnumerable<IGrouping<object, NavigationViewModelBase>> MenuItemsSource { get; }

  private readonly ObservableCollection<NavigationViewModelBase> _menuItems;
  public ReadOnlyObservableCollection<NavigationViewModelBase> MenuItems { get; }

  [ObservableProperty]
  public partial NavigationViewModelBase? CurrentNavigationViewModel { get; set; }

  #region Object Lifetime Management
  public MainViewModel(NavigationService navigationService, NavigationViewModelProvider navigationViewModelProvider, [FromKeyedServices(CommandServiceType.Navigation)] ICommandService navigationCommandService)
  {
    // DI
    NavigationService = navigationService;
    NavigationViewModelProvider = navigationViewModelProvider;
    NavigationCommandService = (NavigationCommandService)navigationCommandService;

    // Header
    HeaderMenuItems = [.. NavigationViewModelProvider.Resolve(NavigationService.PrimaryCoreNavigations)];

    // User
    UserRootNavigationViewModel = (UserRootNavigationViewModel)NavigationViewModelProvider.Resolve(NavigationService.UserRootNavigation);

    // Footer
    FooterMenuItems = [.. NavigationViewModelProvider.Resolve(NavigationService.SecondaryCoreNavigations)];

    // For CollectionViewSource.Source
    //MenuItemsSource = ImmutableArray.Create(new Grouping<object, NavigationViewModelBase>("Header", HeaderMenuItems), new Grouping<object, NavigationViewModelBase>("User", [UserRootNavigationViewModel]));

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
      NavigationService.ResetCurrentNavigation();
    }

    base.Dispose(disposing);
  }
  #endregion

  public void SetNavigation(NavigationId navigationId)
  {
    NavigationViewModelBase? viewmodel =
      HeaderMenuItems.FirstOrDefault(vm => vm.Navigation is NavigationCoreNode coreNode && coreNode.Id == navigationId)
      ?? FooterMenuItems.FirstOrDefault(vm => vm.Navigation is NavigationCoreNode coreNode && coreNode.Id == navigationId)
      ?? UserRootNavigationViewModel.FirstDescendantOrDefault(vm => vm.Navigation is NavigationUserNode userNode && userNode.Id == navigationId, false)
      ?? (HeaderMenuItems.Count > 0 ? HeaderMenuItems[0] : null);

    if (viewmodel is not null)
    {
      NavigationService.PushNavigation(viewmodel.Navigation);
    }
  }

  private void NavigationService_CurrentNavigationChanged(object sender, INavigation? args)
  {
    CurrentNavigationViewModel = args switch
    {
      INavigationNode node when NavigationViewModelProvider.TryResolve(node, out var viewmodel) => viewmodel,
      NavigationSearch search => NavigationViewModelProvider.Resolve(search),
      _ => null
    };
  }

  public void PushNavigation(INavigation navigation)
  {
    NavigationService.PushNavigation(navigation);
  }

  public void PopNavigation()
  {
    NavigationService.PopNavigationBackStack();
  }

  public void SynchronizeNavigation()
  {
    CurrentNavigationViewModel ??= NavigationService.CurrentNavigation switch
      {
        INavigationNode node when NavigationViewModelProvider.TryResolve(node, out var viewmodel) => viewmodel,
        NavigationSearch search => NavigationViewModelProvider.Resolve(search),
        _ => null
      };
  }

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

        PushNavigation(navigationSearch);
      }
    };
  }
}

/// <eventsubscription>
/// </eventsubscription>

public class Grouping<TKey, TElement>(TKey key, IEnumerable<TElement> items) : IGrouping<TKey, TElement>
{
  public TKey Key { get; } = key;
  public IEnumerable<TElement> Items { get; } = items;

  public IEnumerator<TElement> GetEnumerator() => Items.GetEnumerator();

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
