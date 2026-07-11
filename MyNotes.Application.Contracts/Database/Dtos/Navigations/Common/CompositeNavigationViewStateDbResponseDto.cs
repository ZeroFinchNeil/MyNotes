namespace MyNotes.Application.Contracts.Database.Dtos.Navigations.Common;

internal sealed record CompositeNavigationViewStateDbResponseDto : NavigationViewStateDbResponseDto
{
  public required bool IsExpanded { get; init; }
}