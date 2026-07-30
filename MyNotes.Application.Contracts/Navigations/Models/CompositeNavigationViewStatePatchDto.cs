using DotNext;

namespace MyNotes.Application.Contracts.Navigations.Models;

internal sealed record CompositeNavigationViewStatePatchDto : NavigationViewStatePatchDto
{
  public Optional<bool> IsExpanded { get; init; }

  public override bool IsEmpty => this is
  {
    IsExpanded.IsUndefined: true,
  };
}