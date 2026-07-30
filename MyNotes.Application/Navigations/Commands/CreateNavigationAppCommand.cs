using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Domain.Navigations;

namespace MyNotes.Application.Navigations.Commands;

internal sealed record CreateNavigationAppCommand
{
  public required bool IsComposite { get; init; }

  public required int Icon { get; init; }

  public required string Title { get; init; }

  public required NavigationId InsertTargetId { get; init; }

  public required NavigationInsertPosition InsertPosition { get; init; }
}