namespace MyNotes.Application.Dtos.Navigations.Common;

internal sealed record UserCompositeNavigationBundleAppResponseDto : UserNavigationBundleAppResponseDto
{
  public override UserCompositeNavigationAppResponseDto UserNavigationDto { get; }

  public override UserCompositeNavigationViewStateAppResponseDto ViewStateDto { get; }

  public UserCompositeNavigationBundleAppResponseDto(UserCompositeNavigationAppResponseDto userNavigationDto, UserCompositeNavigationViewStateAppResponseDto viewStateDto, ImmutableList<UserNavigationBundleAppResponseDto> children) : base(userNavigationDto, viewStateDto)
  {
    UserNavigationDto = userNavigationDto;
    ViewStateDto = viewStateDto;
    Children = children;
  }

  public UserCompositeNavigationBundleAppResponseDto(UserCompositeNavigationAppResponseDto userNavigationDto, UserCompositeNavigationViewStateAppResponseDto viewStateDto, IReadOnlyList<UserNavigationBundleAppResponseDto> children) : base(userNavigationDto, viewStateDto)
  {
    UserNavigationDto = userNavigationDto;
    ViewStateDto = viewStateDto;
    Children = [.. children];
  }

  public ImmutableList<UserNavigationBundleAppResponseDto> Children
  {
    get;
    init
    {
      ArgumentNullException.ThrowIfNull(value);
      field = value;
    }
  }
}