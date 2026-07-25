namespace MyNotes.Application.Contracts.Models.Navigations.Common;

internal sealed record CompositeNavigationViewStateDbResponseDto : NavigationViewStateDbResponseDto
{
  public required bool IsExpanded { get; init; }
}