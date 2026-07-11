namespace MyNotes.Application.Dtos.Navigations.Common;

internal sealed record CompositeNavigationViewStateAppResponseDto : NavigationViewStateAppResponseDto
{
  public required bool IsExpanded { get; init; }
}