using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Commands;
using MyNotes.Debugging;
using MyNotes.Models.Navigations;
using MyNotes.Services.Commands;
using MyNotes.Services.Navigations;
using MyNotes.Services.Search;
using MyNotes.ViewModels.Navigations;

namespace MyNotes.ViewModels;

internal sealed partial class MainViewModel : ViewModelBase
{
  private readonly NavigationService NavigationService;
  private readonly NavigationViewModelProvider NavigationViewModelProvider;
  private readonly NavigationViewModelCommandService NavigationViewModelCommandService;

  // Header
  public IReadOnlyList<NavigationViewModelBase> HeaderMenuItems { get; }

  // User
  public UserRootNavigationViewModel UserRootNavigationViewModel { get; }
  public IReadOnlyList<NavigationViewModelBase>? UserNavigationViewModels => UserRootNavigationViewModel.ChildNodeViewModels;

  // Footer
  public IReadOnlyList<NavigationViewModelBase> FooterMenuItems { get; }

  // For CollectionViewSource.Source
  public IReadOnlyList<IReadOnlyList<NavigationViewModelBase>?> MenuItemsSource { get; }

  public NavigationViewModelBase? CurrentNavigationViewModel
  {
    get;
    set => SetProperty(ref field, value);
  }

  public MainViewModel(NavigationService navigationService, NavigationViewModelProvider navigationViewModelProvider, SearchService searchService, [FromKeyedServices(CommandServiceType.NavigationViewModel)] ICommandService navigationViewModelCommandService)
  {
#if DEBUG
    ReferenceTracker.MainViewModelReference.Add(this, GetHashCode());
#endif
    // DI
    NavigationService = navigationService;
    NavigationViewModelProvider = navigationViewModelProvider;
    NavigationViewModelCommandService = (NavigationViewModelCommandService)navigationViewModelCommandService;

    // Header
    HeaderMenuItems = [.. NavigationService.PrimaryCoreNavigations.Select(n => NavigationViewModelProvider.Resolve(n))];

    // User
    UserRootNavigationViewModel = (UserRootNavigationViewModel)NavigationViewModelProvider.Resolve(NavigationService.UserRootNavigation);

    // Footer
    FooterMenuItems = [.. NavigationService.SecondaryCoreNavigations.Select(n => NavigationViewModelProvider.Resolve(n))];

    // For CollectionViewSource.Source
    MenuItemsSource = [HeaderMenuItems, UserNavigationViewModels];

    NavigationService.CurrentNavigationChanged += NavigationService_CurrentNavigationChanged;

    SetCommands();
  }

  public async Task SetInitialPageViewModel(NavigationViewModelBase initialViewModel)
  {
    await NavigationService.BuildNavigationTask;
    CurrentNavigationViewModel = initialViewModel;
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

  protected override void Dispose(bool disposing)
  {
    if (_disposed)
      return;

    if (disposing)
    {
      NavigationViewModelProvider.ReleaseAll();
      NavigationService.CurrentNavigationChanged -= NavigationService_CurrentNavigationChanged;
      NavigationService.ResetCurrentNavigation();
    }

    _disposed = true;
  }
}

internal sealed partial class MainViewModel : ViewModelBase
{
  public Command<NavigationViewModelBase> AddListCommand => NavigationViewModelCommandService.AddListCommand;
  public Command<NavigationViewModelBase> AddGroupCommand => NavigationViewModelCommandService.AddGroupCommand;

  public Command<string>? SearchNoteCommand { get; private set; }

  private void SetCommands()
  {
    SearchNoteCommand = new(
      actionToExecute: async (searchText) =>
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
      });
  }
}

/// <eventsubscription>
/// </eventsubscription>