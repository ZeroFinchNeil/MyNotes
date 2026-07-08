using MyNotes.Application.Contracts.Database.Dtos.Navigations.Modification;
using MyNotes.Application.Contracts.Database.Enums.Navigations;
using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Application.Dtos.Navigations.Modification;
using MyNotes.Application.Mappers;
using MyNotes.Domain.Entities.Navigations;
using MyNotes.Domain.ValueObjects;
using MyNotes.Templates;

namespace MyNotes.Application.Services.Navigations;

internal sealed partial class NavigationModificationService
{
  private readonly INavigationRepository NavigationRepository;
  private readonly NavigationArrangementService NavigationArrangementService;
  private readonly UserNavigationFactory UserNavigationFactory;

  public NavigationModificationService(INavigationRepository navigationRepository, NavigationArrangementService navigationArrangementService, UserNavigationFactory userNavigationFactory)
  {
    NavigationRepository = navigationRepository;
    NavigationArrangementService = navigationArrangementService;
    UserNavigationFactory = userNavigationFactory;
  }

  public async Task<UpdateUserNavigationAppResponseDto> UpdateUserNavigationAsync(UpdateUserNavigationAppRequestDto updateUserNavigationAppRequestDto, CancellationToken cancellationToken = default)
  {
    if (updateUserNavigationAppRequestDto.NavigationUpdateField is UserNavigationUpdateFields.None)
    {
      return new UpdateUserNavigationAppResponseDto()
      {
        Id = updateUserNavigationAppRequestDto.Id,
        ChangedNavigationFields = UserNavigationChangedFields.None
      };
    }

    var bundleDto = await NavigationRepository.GetUserNavigationByIdAsync(updateUserNavigationAppRequestDto.Id, cancellationToken)
      ?? throw new InvalidOperationException();

    UserNavigation userNavigation = UserNavigationFactory.Create(bundleDto.UserNavigationDto);
    var changedFields = UpdateUserNavigation(userNavigation, updateUserNavigationAppRequestDto);

    UpdateUserNavigationDbResponseDto updateUserNavigationDbResponseDto = await NavigationRepository.UpdateUserNavigationAsync(UserNavigationMappers.ToUpdateDbDto(userNavigation, UserNavigationMappers.ToUpdateFields(changedFields)), true, cancellationToken);

    return UserNavigationMappers.ToAppDto(updateUserNavigationDbResponseDto);
  }

  private static UserNavigationChangedFields UpdateUserNavigation(UserNavigation userNavigation, UpdateUserNavigationAppRequestDto dto)
  {
    UserNavigationChangedFields changedFields = UserNavigationChangedFields.None;
    var updateFields = dto.NavigationUpdateField;
    if (updateFields.HasFlag(UserNavigationUpdateFields.Parent) && dto.Parent is NavigationId parent && userNavigation.Parent != parent)
    {
      userNavigation.Parent = parent;
      changedFields |= UserNavigationChangedFields.Parent;
    }
    if (updateFields.HasFlag(UserNavigationUpdateFields.Icon) && dto.Icon is Icon icon && userNavigation.Icon != (int)icon)
    {
      userNavigation.Icon = (int)icon;
      changedFields |= UserNavigationChangedFields.Icon;
    }
    if (updateFields.HasFlag(UserNavigationUpdateFields.Title) && dto.Title is string title && userNavigation.Title != title)
    {
      userNavigation.Title = title;
      changedFields |= UserNavigationChangedFields.Title;
    }
    if (updateFields.HasFlag(UserNavigationUpdateFields.IsDeleted) && dto.IsDeleted is bool isDeleted && userNavigation.IsDeleted != isDeleted)
    {
      userNavigation.IsDeleted = isDeleted;
      changedFields |= UserNavigationChangedFields.IsDeleted;
    }

    return changedFields;
  }

  public async Task<bool> DeleteUserNavigationAsync(DeleteUserNavigationAppRequestDto deleteUserNavigationAppRequestDto)
    => await NavigationRepository.DeleteUserNavigationAsync(UserNavigationMappers.ToDbDto(deleteUserNavigationAppRequestDto));
}
