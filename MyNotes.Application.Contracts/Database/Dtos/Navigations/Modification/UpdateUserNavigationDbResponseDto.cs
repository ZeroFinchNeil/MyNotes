using MyNotes.Application.Contracts.Database.Enums.Navigations;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Dtos.Navigations.Modification;

internal sealed record UpdateUserNavigationDbResponseDto
{
  public required NavigationId Id { get; init; }

  public required UserNavigationChangedFields ChangedNavigationFields { get; init; }

  public NavigationId? Parent { get; set; }

  public int? Icon { get; set; }

  public string? Title { get; set; }

  public bool? IsDeleted { get; set; }
}