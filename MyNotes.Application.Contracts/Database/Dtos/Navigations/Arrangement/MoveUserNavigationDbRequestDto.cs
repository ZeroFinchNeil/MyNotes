using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Dtos.Navigations.Arrangement;

internal sealed record MoveUserNavigationDbRequestDto
{
  public required NavigationId SourceNavigation { get; init; }

  public required NavigationId TargetNavigation { get; init; }

  public required NavigationInsertPosition NavigationInsertPosition { get; init; }
}
