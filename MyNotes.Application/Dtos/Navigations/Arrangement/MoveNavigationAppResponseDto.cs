using MyNotes.Application.Enums.Navigations;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Dtos.Navigations.Arrangement;

internal sealed record MoveNavigationAppResponseDto
{
  public required MoveNavigationResultKind ResultKind { get; init; }

  public required IReadOnlyList<NavigationId>? UpdatedNavigations { get; init; }

  public bool IsMoveApplied => ResultKind is not MoveNavigationResultKind.Rejected && UpdatedNavigations is not null;

  public string? FailureMessage { get; init; }
}