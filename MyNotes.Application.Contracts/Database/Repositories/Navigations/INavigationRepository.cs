using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MyNotes.Application.Contracts.Database.Core;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Arrangement;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Common;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Creation;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Modification;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Retrieval;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Repositories.Navigations;

internal interface INavigationRepository
{
  public Task<NavigationId> GenerateUniqueUserNavigationIdAsync(CancellationToken cancellationToken = default);

  public Task<IReadOnlyList<UserNavigationBundleDbResponseDto>> GetAllUserNavigationsAsync(CancellationToken cancellationToken = default);

  public Task<UserNavigationBundleDbResponseDto?> GetUserNavigationByIdAsync(NavigationId id, CancellationToken cancellationToken = default);

  public Task<GetUserNavigationFieldValuesDbResponseDto> GetUserNavigationFieldValuesAsync(GetUserNavigationFieldValuesDbRequestDto getUserNavigationFieldsDbRequestDto, CancellationToken cancellationToken = default);

  public Task<IReadOnlyList<GetUserNavigationFieldValuesDbResponseDto>> MoveUserNavigationAsync(MoveUserNavigationDbRequestDto moveUserNavigationDbRequestDto, IAppDbTransactionContext appDbTransactionContext, CancellationToken cancellationToken = default);

  public Task<UserNavigationBundleDbResponseDto> AddUserNavigationAsync(CreateUserNavigationDbRequestDto createUserNavigationDbRequestDto, IAppDbTransactionContext appDbTransactionContext, CancellationToken cancellationToken = default);

  public Task<UpdateUserNavigationDbResponseDto> UpdateUserNavigationAsync(UpdateUserNavigationDbRequestDto updateNoteDbDto, bool updateIfChanged = true, CancellationToken cancellationToken = default);

  public Task<bool> DeleteUserNavigationAsync(DeleteUserNavigationDbRequestDto deleteUserNavigationDbRequestDto, CancellationToken cancellationToken = default);
}
