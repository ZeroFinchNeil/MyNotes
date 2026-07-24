namespace MyNotes.Application.Contracts.Navigations.Models.Common;

internal sealed record CompositeNavigationViewStateDbResponseDto : NavigationViewStateDbResponseDto
{
  public required bool IsExpanded { get; init; }
}