using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Models.Navigations.Arrangement;

internal sealed record MoveNavigationDbRequestDto
{
  public required NavigationId SourceNavigation { get; init; }

  public required NavigationId TargetNavigation { get; init; }

  public required NavigationInsertPosition InsertPosition { get; init; }
}
