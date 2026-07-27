using MyNotes.Application.Commands.Navigations;
using MyNotes.Application.Contracts.Persistence;
using MyNotes.Application.Contracts.Persistence.Navigations;
using MyNotes.Application.Results;

namespace MyNotes.Application.Services.Navigations;

internal sealed partial class NavigationModificationService
{
  private readonly INavigationRepository NavigationRepository;

  public NavigationModificationService(INavigationRepository navigationRepository)
  {
    NavigationRepository = navigationRepository;
  }

  public async Task<AppUpdateStatus> UpdateNavigationAsync(UpdateNavigationAppCommand appCommand, CancellationToken cancellationToken = default) =>
    await NavigationRepository.UpdateNavigationAsync(appCommand.PatchDto, cancellationToken) switch
    {
      PersistenceMutationStatus.Applied => AppUpdateStatus.Succeeded,
      PersistenceMutationStatus.Expired or PersistenceMutationStatus.Unchanged => AppUpdateStatus.Unchanged,
      PersistenceMutationStatus.NotFound => AppUpdateStatus.TargetNotFound,
      PersistenceMutationStatus.Failed => AppUpdateStatus.Failed,
      _ => throw new InvalidOperationException()
    };

  public async Task<AppUpdateStatus> UpdateNavigationViewStateAsync(UpdateNavigationViewStateAppCommand appCommand, CancellationToken cancellationToken = default) =>
    await NavigationRepository.UpdateNavigationViewStateAsync(appCommand.PatchDto, cancellationToken) switch
    {
      PersistenceMutationStatus.Applied => AppUpdateStatus.Succeeded,
      PersistenceMutationStatus.Expired or PersistenceMutationStatus.Unchanged => AppUpdateStatus.Unchanged,
      PersistenceMutationStatus.NotFound => AppUpdateStatus.TargetNotFound,
      PersistenceMutationStatus.Failed => AppUpdateStatus.Failed,
      _ => throw new InvalidOperationException()
    };

  public async Task<AppUpdateStatus> DeleteNavigationAsync(DeleteNavigationAppCommand appCommand, CancellationToken cancellationToken = default)
    => await NavigationRepository.DeleteNavigationAsync(appCommand.Id, appCommand.DeleteMode, cancellationToken) switch
    {
      PersistenceMutationStatus.Applied => AppUpdateStatus.Succeeded,
      PersistenceMutationStatus.Expired or PersistenceMutationStatus.Unchanged => AppUpdateStatus.Unchanged,
      PersistenceMutationStatus.NotFound => AppUpdateStatus.TargetNotFound,
      PersistenceMutationStatus.Failed => AppUpdateStatus.Failed,
      _ => throw new InvalidOperationException()
    };
}
