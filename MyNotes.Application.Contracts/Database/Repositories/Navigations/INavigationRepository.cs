using System.Collections.Generic;
using System.Threading.Tasks;

using MyNotes.Application.Contracts.Database.Dtos.Navigations;
using MyNotes.Common.Enums.Modes;
using MyNotes.Common.Structures;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Repositories.Navigations;

internal interface INavigationRepository
{
  public Task<NavigationId> GenerateUniqueUserNavigationIdAsync();

  public Task<IReadOnlyList<UserNavigationDbResponseDto>> GetUserNavigationsAsync();

  public Task<GetUserNavigationFieldValuesDbResponseDto> GetUserNavigationFieldsAsync(GetUserNavigationFieldValuesDbRequestDto getUserNavigationFieldsDbRequestDto);

  public Task<IReadOnlyList<GetUserNavigationFieldValuesDbResponseDto>> MoveUserNavigationAsync(MoveUserNavigationDbRequestDto moveUserNavigationDbRequestDto, IDbTransaction? transaction = null);

  public Task<UserNavigationDbAggregateResponseDto> AddUserNavigationAsync(CreateUserNavigationDbRequestDto createUserNavigationDbRequestDto);

  public Task<UpdateUserNavigationDbResponseDto> UpdateUserNavigationAsync(UpdateUserNavigationDbRequestDto updateNoteDbDto, bool updateIfChanged = true);

  public Task<bool> DeleteUserNavigationAsync(DeleteUserNavigationDbRequestDto deleteUserNavigationDbRequestDto);
}
