using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Dtos.Navigations;

internal sealed record MoveUserNavigationAppRequestDto
{
  public required NavigationId SourceNavigation { get; init; }

  public required NavigationId TargetNavigation { get; init; }

  public required NavigationInsertPosition NavigationInsertPosition { get; init; }

  public required IReadOnlyList<NavigationId> ExpectedTargetSiblings { get; init; }
}