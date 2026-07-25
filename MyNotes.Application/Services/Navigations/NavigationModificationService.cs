using MyNotes.Application.Contracts.Enums.Navigations;
using MyNotes.Application.Contracts.Models.Navigations.Modification;
using MyNotes.Application.Contracts.Persistence.Navigations;
using MyNotes.Application.Dtos.Navigations.Modification;
using MyNotes.Application.Mappers;
using MyNotes.Domain.Entities.Navigations;
using MyNotes.Domain.ValueObjects;
using MyNotes.Templates;

namespace MyNotes.Application.Services.Navigations;

internal sealed partial class NavigationModificationService
{
  private readonly INavigationRepository NavigationRepository;
  private readonly NavigationFactory NavigationFactory;

  public NavigationModificationService(INavigationRepository navigationRepository, NavigationFactory navigationFactory)
  {
    NavigationRepository = navigationRepository;
    NavigationFactory = navigationFactory;
  }

  public async Task<UpdateNavigationAppResponseDto> UpdateNavigationAsync(UpdateNavigationAppRequestDto updateAppRequestDto, CancellationToken cancellationToken = default)
  {
    if (updateAppRequestDto.UpdateFields is NavigationUpdateFields.None)
    {
      return new UpdateNavigationAppResponseDto()
      {
        Id = updateAppRequestDto.Id,
        ChangedFields = NavigationChangedFields.None
      };
    }

    var bundleDto = await NavigationRepository.GetNavigationByIdAsync(updateAppRequestDto.Id, cancellationToken)
      ?? throw new InvalidOperationException();

    Navigation navigation = NavigationFactory.Create(bundleDto.NavigationDto);
    var changedFields = UpdateNavigation(navigation, updateAppRequestDto);

    UpdateNavigationDbResponseDto updateDbResponseDto = await NavigationRepository.UpdateNavigationAsync(NavigationMappers.ToUpdateDbDto(navigation, NavigationMappers.ToUpdateFields(changedFields)), true, cancellationToken);

    return NavigationMappers.ToAppDto(updateDbResponseDto);
  }

  private static NavigationChangedFields UpdateNavigation(Navigation navigation, UpdateNavigationAppRequestDto updateAppRequestDto)
  {
    NavigationChangedFields changedFields = NavigationChangedFields.None;
    var updateFields = updateAppRequestDto.UpdateFields;
    if (updateFields.HasFlag(NavigationUpdateFields.Parent) && updateAppRequestDto.Parent is NavigationId parent && navigation.Parent != parent)
    {
      navigation.Parent = parent;
      changedFields |= NavigationChangedFields.Parent;
    }
    if (updateFields.HasFlag(NavigationUpdateFields.Icon) && updateAppRequestDto.Icon is Icon icon && navigation.Icon != (int)icon)
    {
      navigation.Icon = (int)icon;
      changedFields |= NavigationChangedFields.Icon;
    }
    if (updateFields.HasFlag(NavigationUpdateFields.Title) && updateAppRequestDto.Title is string title && navigation.Title != title)
    {
      navigation.Title = title;
      changedFields |= NavigationChangedFields.Title;
    }
    if (updateFields.HasFlag(NavigationUpdateFields.IsDeleted) && updateAppRequestDto.IsDeleted is bool isDeleted && navigation.IsDeleted != isDeleted)
    {
      navigation.IsDeleted = isDeleted;
      changedFields |= NavigationChangedFields.IsDeleted;
    }

    return changedFields;
  }

  public Task UpdateNavigationViewStateAsync(UpdateNavigationViewStateAppRequestDto updateAppRequestDto, CancellationToken cancellationToken = default) => NavigationRepository.UpdateNavigationViewStateAsync(NavigationMappers.ToDbDto(updateAppRequestDto), true, cancellationToken);

  public async Task<bool> DeleteNavigationAsync(DeleteNavigationAppRequestDto deleteAppRequestDto)
    => await NavigationRepository.DeleteNavigationAsync(NavigationMappers.ToDbDto(deleteAppRequestDto));
}
