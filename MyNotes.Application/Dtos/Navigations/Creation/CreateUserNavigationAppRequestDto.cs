using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Domain.ValueObjects;
using MyNotes.Templates;

namespace MyNotes.Application.Dtos.Navigations.Creation;

internal sealed record CreateUserNavigationAppRequestDto
{
  public required NavigationId InsertTargetId { get; init; }

  public required NavigationInsertPosition NavigationInsertPosition { get; init; }

  public required bool IsComposite { get; init; }

  public required Icon Icon { get; init; }

  public required string Title { get; init; }
}