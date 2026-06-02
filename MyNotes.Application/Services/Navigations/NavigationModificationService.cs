using MyNotes.Application.Contracts.Database.Dtos.Navigations.Modification;
using MyNotes.Application.Contracts.Database.Enums.Navigations;
using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Application.Dtos.Navigations.Modification;
using MyNotes.Application.Mappers;
using MyNotes.Domain.Entities.Navigations;

namespace MyNotes.Application.Services.Navigations;

internal sealed partial class NavigationModificationService
{
  private readonly INavigationRepository NavigationRepository;
  private readonly NavigationArrangementService NavigationArrangementService;

  public NavigationModificationService(INavigationRepository navigationRepository, NavigationArrangementService navigationArrangementService)
  {
    NavigationRepository = navigationRepository;
    NavigationArrangementService = navigationArrangementService;
  }

  public async Task<UpdateUserNavigationAppResponseDto> UpdateUserNavigationAsync(UpdateUserNavigationAppRequestDto updateUserNavigationAppRequestDto)
  {
    if (updateUserNavigationAppRequestDto.NavigationUpdateField is UserNavigationUpdateFields.None)
    {
      return new UpdateUserNavigationAppResponseDto()
      {
        Id = updateUserNavigationAppRequestDto.Id,
        ChangedNavigationFields = UserNavigationChangedFields.None
      };
    }

    UserNavigation userNavigation = UserNavigationMappers.ToDomainEntity(updateUserNavigationAppRequestDto);

    UpdateUserNavigationDbResponseDto updateUserNavigationDbResponseDto = await NavigationRepository.UpdateUserNavigationAsync(UserNavigationMappers.ToDbDto(userNavigation));

    return UserNavigationMappers.ToAppDto(updateUserNavigationDbResponseDto);
  }

  public async Task<bool> DeleteUserNavigationAsync(DeleteUserNavigationAppRequestDto deleteUserNavigationAppRequestDto)
    => await NavigationRepository.DeleteUserNavigationAsync(UserNavigationMappers.ToDbDto(deleteUserNavigationAppRequestDto));
}
