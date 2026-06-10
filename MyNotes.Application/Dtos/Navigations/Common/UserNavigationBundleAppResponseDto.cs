using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Dtos.Navigations.Common;

internal abstract record UserNavigationBundleAppResponseDto
{
  public NavigationId Id => UserNavigationDto.Id;

  public UserNavigationAppResponseDto UserNavigationDto { get; }

  public UserNavigationViewStateAppResponseDto ViewStateDto { get; }

  protected UserNavigationBundleAppResponseDto(UserNavigationAppResponseDto userNavigationDto, UserNavigationViewStateAppResponseDto viewStateDto)
  {
    if (userNavigationDto.Id != viewStateDto.Id)
    {
      throw new ArgumentException("두 Dto의 Id가 일치하지 않습니다.", nameof(userNavigationDto));
    }

    UserNavigationDto = userNavigationDto;
    ViewStateDto = viewStateDto;
  }

  //public abstract UserNavigationBundleAppResponseDto With(UserNavigationAppResponseDto? userNavigationDto = null, UserNavigationViewStateAppResponseDto? viewStateDto = null);

  //public abstract UserNavigationBundleAppResponseDto With(UserNavigationAppResponseDto? userNavigationDto = null, UserNavigationViewStateAppResponseDto? viewStateDto = null) => new(userNavigationDto ?? UserNavigationDto, viewStateDto ?? ViewStateDto);
}