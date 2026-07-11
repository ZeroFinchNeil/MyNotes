namespace MyNotes.Application.Dtos.Navigations.Common;

internal sealed record CompositeNavigationBundleAppResponseDto : NavigationBundleAppResponseDto
{
  public override CompositeNavigationAppResponseDto NavigationDto { get; }

  public override CompositeNavigationViewStateAppResponseDto ViewStateDto { get; }

  public CompositeNavigationBundleAppResponseDto(CompositeNavigationAppResponseDto navigationDto, CompositeNavigationViewStateAppResponseDto viewStateDto, ImmutableList<NavigationBundleAppResponseDto> children) : base(navigationDto, viewStateDto)
  {
    NavigationDto = navigationDto;
    ViewStateDto = viewStateDto;
    Children = children;
  }

  public CompositeNavigationBundleAppResponseDto(CompositeNavigationAppResponseDto navigationDto, CompositeNavigationViewStateAppResponseDto viewStateDto, IReadOnlyList<NavigationBundleAppResponseDto> children) : base(navigationDto, viewStateDto)
  {
    NavigationDto = navigationDto;
    ViewStateDto = viewStateDto;
    Children = [.. children];
  }

  public ImmutableList<NavigationBundleAppResponseDto> Children
  {
    get;
    init
    {
      ArgumentNullException.ThrowIfNull(value);
      field = value;
    }
  }
}