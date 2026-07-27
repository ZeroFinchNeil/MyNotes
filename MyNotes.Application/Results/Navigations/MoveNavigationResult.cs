using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Results.Navigations;

internal sealed record MoveNavigationResult
{
  public required MoveNavigationResultKind Kind { get; init; }

  public required IReadOnlyList<NavigationId>? UpdatedNavigations { get; init; }

  public string? FailureMessage { get; init; }
}