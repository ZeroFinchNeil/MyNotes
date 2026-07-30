using System.Collections.Generic;

namespace MyNotes.Application.Contracts.Navigations.Models;

internal sealed record CompositeNavigationTreeNodeDto : NavigationTreeNodeDto
{
  public required IReadOnlyList<NavigationTreeNodeDto> Children { get; init; }
}