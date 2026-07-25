using System;

using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Models.Navigations.Common;

internal sealed record NavigationBundleDbResponseDto
{
  public NavigationId Id => NavigationDto.Id;

  public NavigationDbResponseDto NavigationDto { get; }

  public NavigationViewStateDbResponseDto ViewStateDto { get; }

  public NavigationBundleDbResponseDto(NavigationDbResponseDto navigationDto, NavigationViewStateDbResponseDto viewStateDto)
  {
    if (!navigationDto.Id.Equals(viewStateDto.Id))
    {
      throw new ArgumentException("두 Dto의 Id가 일치하지 않습니다.", nameof(navigationDto));
    }

    NavigationDto = navigationDto;
    ViewStateDto = viewStateDto;
  }
}
