namespace MyNotes.Application.Dtos.Navigations.Common;

internal sealed record LeafNavigationBundleAppResponseDto : NavigationBundleAppResponseDto
{
  public override LeafNavigationAppResponseDto NavigationDto { get; }

  public override LeafNavigationViewStateAppResponseDto ViewStateDto { get; }

  public LeafNavigationBundleAppResponseDto(LeafNavigationAppResponseDto navigationDto, LeafNavigationViewStateAppResponseDto viewStateDto) : base(navigationDto, viewStateDto) 
  {
    NavigationDto = navigationDto;
    ViewStateDto = viewStateDto;
  }
}