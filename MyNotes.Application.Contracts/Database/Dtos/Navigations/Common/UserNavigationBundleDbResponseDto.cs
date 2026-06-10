using System;

using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Dtos.Navigations.Common;

internal sealed record UserNavigationBundleDbResponseDto
{
  public NavigationId Id => UserNavigationDto.Id;

  public UserNavigationDbResponseDto UserNavigationDto { get; }

  public UserNavigationViewStateDbResponseDto ViewStateDto { get; }

  public UserNavigationBundleDbResponseDto(UserNavigationDbResponseDto userNavigationDto, UserNavigationViewStateDbResponseDto viewStateDto)
  {
    if (!userNavigationDto.Id.Equals(viewStateDto.Id))
    {
      throw new ArgumentException("두 Dto의 Id가 일치하지 않습니다.", nameof(userNavigationDto));
    }

    UserNavigationDto = userNavigationDto;
    ViewStateDto = viewStateDto;
  }
}
