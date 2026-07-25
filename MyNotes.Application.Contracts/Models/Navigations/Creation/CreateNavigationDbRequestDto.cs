using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Models.Navigations.Creation;

internal sealed record CreateNavigationDbRequestDto
{
  public required NavigationId Id { get; init; }

  public required NavigationId ParentId { get; init; }

  public required NavigationId InsertTargetId { get; init; }

  public required NavigationInsertPosition InsertPosition { get; init; }

  public required bool IsComposite { get; init; }

  public required int Icon { get; init; }

  public required string Title { get; init; }
}