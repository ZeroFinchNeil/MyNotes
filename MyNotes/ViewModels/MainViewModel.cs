using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Commands;
using MyNotes.Debugging;
using MyNotes.Models.Navigations;
using MyNotes.Services.Commands;
using MyNotes.Services.Navigations;
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
    set
    {
      SetProperty(ref field, value);
    }
  }

  public MainViewModel(NavigationService navigationService, NavigationViewModelProvider navigationViewModelProvider, [FromKeyedServices(CommandServiceType.NavigationViewModel)] ICommandService commandService)
  {
#if DEBUG
    ReferenceTracker.MainViewModelReference.Add(this, GetHashCode());
#endif
    // DI
    NavigationService = navigationService;
    NavigationViewModelProvider = navigationViewModelProvider;
    NavigationViewModelCommandService = (NavigationViewModelCommandService)commandService;

    // Header
    HeaderMenuItems = [.. NavigationService.PrimaryCoreNavigations.Select(n => NavigationViewModelProvider.Resolve(n))];

    // User
    UserRootNavigationViewModel = (UserRootNavigationViewModel)NavigationViewModelProvider.Resolve(NavigationService.UserRootNavigation);

    // Footer
    FooterMenuItems = [.. NavigationService.SecondaryCoreNavigations.Select(n => NavigationViewModelProvider.Resolve(n))];

    // For CollectionViewSource.Source
    MenuItemsSource = [HeaderMenuItems, UserNavigationViewModels];

    NavigationService.CurrentNavigationChanged += NavigationService_CurrentNavigationChanged;

    // Initial Navigation
    CurrentNavigationViewModel = HeaderMenuItems[0];
  }

  private void NavigationService_CurrentNavigationChanged(object sender, INavigation args)
  {
    if (NavigationViewModelProvider.TryResolve(args, out var viewmodel))
      CurrentNavigationViewModel = viewmodel;
  }

  public void PushNavigation(INavigation navigation)
  {
    NavigationService.PushNavigationBackStack(navigation);
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
}