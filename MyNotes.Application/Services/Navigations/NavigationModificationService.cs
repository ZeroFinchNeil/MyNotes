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
  private readonly UserNavigationFactory UserNavigationFactory;

  public NavigationModificationService(INavigationRepository navigationRepository, UserNavigationFactory userNavigationFactory)
  {
    NavigationRepository = navigationRepository;
    UserNavigationFactory = userNavigationFactory;
  }

  public async Task<UpdateUserNavigationAppResponseDto> UpdateUserNavigationAsync(UpdateUserNavigationAppRequestDto updateAppRequestDto, CancellationToken cancellationToken = default)
  {
    if (updateAppRequestDto.NavigationUpdateField is UserNavigationUpdateFields.None)
    {
      return new UpdateUserNavigationAppResponseDto()
      {
        Id = updateAppRequestDto.Id,
        ChangedNavigationFields = UserNavigationChangedFields.None
      };
    }

    var bundleDto = await NavigationRepository.GetUserNavigationByIdAsync(updateAppRequestDto.Id, cancellationToken)
      ?? throw new InvalidOperationException();

    UserNavigation userNavigation = UserNavigationFactory.Create(bundleDto.UserNavigationDto);
    var changedFields = UpdateUserNavigation(userNavigation, updateAppRequestDto);

    UpdateUserNavigationDbResponseDto updateUserNavigationDbResponseDto = await NavigationRepository.UpdateUserNavigationAsync(UserNavigationMappers.ToUpdateDbDto(userNavigation, UserNavigationMappers.ToUpdateFields(changedFields)), true, cancellationToken);

    return UserNavigationMappers.ToAppDto(updateUserNavigationDbResponseDto);
  }

  private static UserNavigationChangedFields UpdateUserNavigation(UserNavigation userNavigation, UpdateUserNavigationAppRequestDto updateAppRequestDto)
  {
    UserNavigationChangedFields changedFields = UserNavigationChangedFields.None;
    var updateFields = updateAppRequestDto.NavigationUpdateField;
    if (updateFields.HasFlag(UserNavigationUpdateFields.Parent) && updateAppRequestDto.Parent is NavigationId parent && userNavigation.Parent != parent)
    {
      userNavigation.Parent = parent;
      changedFields |= UserNavigationChangedFields.Parent;
    }
    if (updateFields.HasFlag(UserNavigationUpdateFields.Icon) && updateAppRequestDto.Icon is Icon icon && userNavigation.Icon != (int)icon)
    {
      userNavigation.Icon = (int)icon;
      changedFields |= UserNavigationChangedFields.Icon;
    }
    if (updateFields.HasFlag(UserNavigationUpdateFields.Title) && updateAppRequestDto.Title is string title && userNavigation.Title != title)
    {
      userNavigation.Title = title;
      changedFields |= UserNavigationChangedFields.Title;
    }
    if (updateFields.HasFlag(UserNavigationUpdateFields.IsDeleted) && updateAppRequestDto.IsDeleted is bool isDeleted && userNavigation.IsDeleted != isDeleted)
    {
      userNavigation.IsDeleted = isDeleted;
      changedFields |= UserNavigationChangedFields.IsDeleted;
    }

    return changedFields;
  }

  public Task UpdateUserNavigationViewStateAsync(UpdateUserNavigationViewStateAppRequestDto updateAppRequestDto, CancellationToken cancellationToken = default) => NavigationRepository.UpdateUserNavigationViewStateAsync(UserNavigationMappers.ToDbDto(updateAppRequestDto), true, cancellationToken);

  public async Task<bool> DeleteUserNavigationAsync(DeleteUserNavigationAppRequestDto deleteAppRequestDto)
    => await NavigationRepository.DeleteUserNavigationAsync(UserNavigationMappers.ToDbDto(deleteAppRequestDto));
}
