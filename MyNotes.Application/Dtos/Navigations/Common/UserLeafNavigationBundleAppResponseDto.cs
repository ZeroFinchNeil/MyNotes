namespace MyNotes.Application.Dtos.Navigations.Common;

internal sealed record UserLeafNavigationBundleAppResponseDto : UserNavigationBundleAppResponseDto
{
  public override UserLeafNavigationAppResponseDto UserNavigationDto { get; }

  public override UserLeafNavigationViewStateAppResponseDto ViewStateDto { get; }

  public UserLeafNavigationBundleAppResponseDto(UserLeafNavigationAppResponseDto userNavigationDto, UserLeafNavigationViewStateAppResponseDto viewStateDto) : base(userNavigationDto, viewStateDto) 
  {
    UserNavigationDto = userNavigationDto;
    ViewStateDto = viewStateDto;
  }
}