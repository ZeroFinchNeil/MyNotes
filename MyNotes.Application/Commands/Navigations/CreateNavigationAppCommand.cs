using MyNotes.Application.Contracts.Enums.Navigations;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Commands.Navigations;

internal sealed record CreateNavigationAppCommand
{
  public required bool IsComposite { get; init; }

  public required int Icon { get; init; }

  public required string Title { get; init; }

  public required NavigationId InsertTargetId { get; init; }

  public required NavigationInsertPosition InsertPosition { get; init; }
}