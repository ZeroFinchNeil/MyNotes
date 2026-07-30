using MyNotes.Domain.Navigations;

namespace MyNotes.Application.Navigations.Results;

internal sealed record MoveNavigationResult
{
  public required MoveNavigationResultKind Kind { get; init; }

  public required IReadOnlyList<NavigationId>? UpdatedNavigations { get; init; }

  public string? FailureMessage { get; init; }
}