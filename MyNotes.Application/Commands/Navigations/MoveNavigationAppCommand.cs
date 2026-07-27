using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Commands.Navigations;

internal sealed record MoveNavigationAppCommand
{
  public required NavigationId SourceNavigationId { get; init; }
  
  public required NavigationId TargetNavigationId { get; init; }
  
  public required NavigationInsertPosition InsertPosition { get; init; }

  public required IReadOnlyList<NavigationId> ExpectedTargetSiblings { get; init; }
}