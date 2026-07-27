namespace MyNotes.Application.Contracts.Models.Navigations;

internal sealed record CompositeNavigationViewStateDto : NavigationViewStateDto
{
  public required bool IsExpanded { get; init; }
}