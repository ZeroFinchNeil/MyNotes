using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Commands;
using MyNotes.Models.Navigations;
using MyNotes.Services.Commands;

namespace MyNotes.ViewModels.Navigations;

internal sealed partial class UserLeafNavigationViewModel : UserNavigationViewModel
{
  public override NavigationUserLeafNode Navigation { get; }

  private readonly NavigationCommandService NavigationCommandService;
  private readonly IServiceScope ServiceScope;

  public UserLeafNavigationViewModel([FromKeyedServices(CommandServiceType.Navigation)] ICommandService navigationCommandService, IServiceScope serviceScope, NavigationUserLeafNode navigation)
  {
    ServiceScope = serviceScope;
    Navigation = navigation;

    // Dependency Injection
    NavigationCommandService = (NavigationCommandService)navigationCommandService;
  }

  protected override void Dispose(bool disposing)
  {
    if (_disposed)
      return;

    if (disposing)
    {
      ServiceScope.Dispose();
    }

    _disposed = true;
  }

  public override Command<NavigationViewModelBase>? AddListCommand => NavigationCommandService.AddListCommand;
  public override Command<NavigationViewModelBase>? AddGroupCommand => NavigationCommandService.AddGroupCommand;
  public override Command<NavigationViewModelBase>? UpdateCommand => NavigationCommandService.UpdateCommand;
  public override Command<NavigationViewModelBase>? DeleteCommand => NavigationCommandService.DeleteCommand;
  public override Command<(NavigationViewModelBase SourceItemViewModel, NavigationViewModelBase TargetGroupViewModel)>? MoveToGroupCommand => NavigationCommandService.MoveToGroupCommand;
}
