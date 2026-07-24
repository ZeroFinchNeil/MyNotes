using MyNotes.Application.Contracts.Navigations.Enums;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Navigations.Models.Modification;

internal sealed record UpdateNavigationDbResponseDto
{
  public required NavigationId Id { get; init; }

  public required NavigationChangedFields ChangedFields { get; init; }

  public NavigationId? Parent { get; set; }

  public int? Icon { get; set; }

  public string? Title { get; set; }

  public bool? IsDeleted { get; set; }
}