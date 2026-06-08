namespace MyNotes.Application.Dtos.Navigations.Common;

internal sealed record UserCompositeNavigationBundleAppResponseDto : UserNavigationBundleAppResponseDto
{
  public UserCompositeNavigationBundleAppResponseDto(UserCompositeNavigationAppResponseDto userNavigationDto, UserCompositeNavigationViewStateAppResponseDto viewStateDto, ImmutableList<UserNavigationBundleAppResponseDto> children) : base(userNavigationDto, viewStateDto)
  {
    Children = children;
  }

  public UserCompositeNavigationBundleAppResponseDto(UserCompositeNavigationAppResponseDto userNavigationDto, UserCompositeNavigationViewStateAppResponseDto viewStateDto, IReadOnlyList<UserNavigationBundleAppResponseDto> children) : base(userNavigationDto, viewStateDto)
  {
    Children = [.. children];
  }

  public ImmutableList<UserNavigationBundleAppResponseDto> Children
  {
    get => field;
    init
    {
      ArgumentNullException.ThrowIfNull(value);
      field = value;
    }
  }
}