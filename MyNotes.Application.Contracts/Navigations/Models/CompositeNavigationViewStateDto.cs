namespace MyNotes.Application.Contracts.Navigations.Models;

internal sealed record CompositeNavigationViewStateDto : NavigationViewStateDto
{
  public required bool IsExpanded { get; init; }
}