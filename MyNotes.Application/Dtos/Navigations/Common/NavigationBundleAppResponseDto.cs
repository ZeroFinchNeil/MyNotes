using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Dtos.Navigations.Common;

internal abstract record NavigationBundleAppResponseDto
{
  public NavigationId Id => NavigationDto.Id;

  public abstract NavigationAppResponseDto NavigationDto { get; }

  public abstract NavigationViewStateAppResponseDto ViewStateDto { get; }

  protected NavigationBundleAppResponseDto(NavigationAppResponseDto navigationDto, NavigationViewStateAppResponseDto viewStateDto)
  {
    if (navigationDto.Id != viewStateDto.Id)
    {
      throw new ArgumentException("두 Dto의 Id가 일치하지 않습니다.", nameof(navigationDto));
    }
  }
}