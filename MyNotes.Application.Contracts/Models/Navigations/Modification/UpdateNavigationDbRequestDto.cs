using MyNotes.Application.Contracts.Enums.Navigations;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Models.Navigations.Modification;

internal sealed record UpdateNavigationDbRequestDto
{
  public required NavigationId Id { get; init; }

  public required NavigationUpdateFields UpdateFields { get; init; }

  public NavigationId? Parent { get; set; }

  public required bool IsComposite { get; init; }

  public int? Icon { get; set; }

  public string? Title { get; set; }

  public bool? IsDeleted { get; set; }
}