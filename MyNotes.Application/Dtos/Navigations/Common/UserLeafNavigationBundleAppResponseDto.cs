namespace MyNotes.Application.Dtos.Navigations.Common;

internal sealed record UserLeafNavigationBundleAppResponseDto : UserNavigationBundleAppResponseDto
{
  public UserLeafNavigationBundleAppResponseDto(UserLeafNavigationAppResponseDto userNavigationDto, UserLeafNavigationViewStateAppResponseDto viewStateDto) : base(userNavigationDto, viewStateDto) { }
}