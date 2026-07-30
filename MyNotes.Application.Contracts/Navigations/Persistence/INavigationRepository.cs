using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MyNotes.Application.Contracts.Database.Core;
using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Application.Contracts.Persistence;
using MyNotes.Common.Enums.Modes;
using MyNotes.Domain.Navigations;

namespace MyNotes.Application.Contracts.Navigations.Persistence;

internal interface INavigationRepository
{
  public Task<NavigationId> GenerateUniqueNavigationIdAsync(CancellationToken cancellationToken = default);

  public Task<IReadOnlyList<NavigationDto>> GetAllNavigationsInSiblingOrderAsync(CancellationToken cancellationToken = default);

  public Task<NavigationDto?> GetNavigationByIdAsync(NavigationId id, CancellationToken cancellationToken = default);

  public Task<NavigationProjectionDto> GetNavigationFieldValuesAsync(NavigationId navigationId, NavigationProjectionFields navigationGetFields, CancellationToken cancellationToken = default);

  public Task<bool> IsDescendantOfAsync(NavigationId possibleDescendantId, NavigationId possibleAncestorId, CancellationToken cancellationToken = default);

  public Task AddNavigationAsync(NavigationDto navigationDto, NavigationId targetNavigationId, NavigationInsertPosition insertPosition, IAppDbTransactionContext appDbTransactionContext, CancellationToken cancellationToken = default);

  public Task<IReadOnlyList<NavigationProjectionDto>> MoveNavigationAsync(NavigationId sourceNavigationId, NavigationId targetNavigationId, NavigationInsertPosition insertPosition, IAppDbTransactionContext appDbTransactionContext, CancellationToken cancellationToken = default);

  public Task<PersistenceMutationStatus> UpdateNavigationAsync(NavigationPatchDto patchDto, CancellationToken cancellationToken = default);

  public Task<PersistenceMutationStatus> UpdateNavigationViewStateAsync(NavigationViewStatePatchDto patchDto, CancellationToken cancellationToken = default);

  public Task<PersistenceMutationStatus> DeleteNavigationAsync(NavigationId navigationId, DeleteMode deleteMode, CancellationToken cancellationToken = default);
}
