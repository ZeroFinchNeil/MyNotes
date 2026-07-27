using System.Collections.Generic;

namespace MyNotes.Application.Contracts.Models.Navigations;

internal sealed record CompositeNavigationTreeNodeDto : NavigationTreeNodeDto
{
  public required IReadOnlyList<NavigationTreeNodeDto> Children { get; init; }
}