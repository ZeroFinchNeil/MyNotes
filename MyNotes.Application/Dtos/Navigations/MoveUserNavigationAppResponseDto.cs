using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Dtos.Navigations;

internal sealed record MoveUserNavigationAppResponseDto
{
  public required bool IsMoveAllowed { get; init; }

  public required IReadOnlyList<NavigationId> UpdatedNavigations { get; init; }

  public string? FailureMessage { get; init; }
}