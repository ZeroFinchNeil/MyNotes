using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Domain.Navigations;

namespace MyNotes.Application.Navigations.Commands;

internal sealed record MoveNavigationAppCommand
{
  public required NavigationId SourceNavigationId { get; init; }

  public required NavigationId TargetNavigationId { get; init; }

  public required NavigationInsertPosition InsertPosition { get; init; }

  public required IReadOnlyList<NavigationId> ExpectedTargetSiblings { get; init; }
}