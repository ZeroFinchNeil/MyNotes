using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MyNotes.Application.Contracts.Database.Core;
using MyNotes.Application.Contracts.Navigations.Models.Arrangement;
using MyNotes.Application.Contracts.Navigations.Models.Common;
using MyNotes.Application.Contracts.Navigations.Models.Creation;
using MyNotes.Application.Contracts.Navigations.Models.Modification;
using MyNotes.Application.Contracts.Navigations.Models.Retrieval;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Navigations.Persistence;

internal interface INavigationRepository
{
  public Task<NavigationId> GenerateUniqueNavigationIdAsync(CancellationToken cancellationToken = default);

  public Task<IReadOnlyList<NavigationBundleDbResponseDto>> GetAllNavigationsAsync(CancellationToken cancellationToken = default);

  public Task<NavigationBundleDbResponseDto?> GetNavigationByIdAsync(NavigationId id, CancellationToken cancellationToken = default);

  public Task<GetNavigationFieldValuesDbResponseDto> GetNavigationFieldValuesAsync(GetNavigationFieldValuesDbRequestDto getFieldsDbRequestDto, CancellationToken cancellationToken = default);

  public Task<bool> IsDescendantOfAsync(NavigationId possibleDescendantId, NavigationId possibleAncestorId, CancellationToken cancellationToken = default);

  public Task<IReadOnlyList<GetNavigationFieldValuesDbResponseDto>> MoveNavigationAsync(MoveNavigationDbRequestDto moveDbRequestDto, IAppDbTransactionContext appDbTransactionContext, CancellationToken cancellationToken = default);

  public Task<NavigationBundleDbResponseDto> AddNavigationAsync(CreateNavigationDbRequestDto createDbRequestDto, IAppDbTransactionContext appDbTransactionContext, CancellationToken cancellationToken = default);

  public Task<UpdateNavigationDbResponseDto> UpdateNavigationAsync(UpdateNavigationDbRequestDto updateNoteDbDto, bool updateIfChanged = true, CancellationToken cancellationToken = default);

  public Task UpdateNavigationViewStateAsync(UpdateNavigationViewStateDbRequestDto updateNoteDbDto, bool updateIfChanged = true, CancellationToken cancellationToken = default);

  public Task<bool> DeleteNavigationAsync(DeleteNavigationDbRequestDto deleteDbRequestDto, CancellationToken cancellationToken = default);
}
