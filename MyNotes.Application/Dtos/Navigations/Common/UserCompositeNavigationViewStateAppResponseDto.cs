namespace MyNotes.Application.Dtos.Navigations.Common;

internal sealed record UserCompositeNavigationViewStateAppResponseDto : UserNavigationViewStateAppResponseDto
{
  public required bool IsExpanded { get; init; }
}