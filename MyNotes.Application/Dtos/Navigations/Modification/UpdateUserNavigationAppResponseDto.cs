using MyNotes.Application.Contracts.Database.Enums.Navigations;
using MyNotes.Domain.ValueObjects;
using MyNotes.Templates;

namespace MyNotes.Application.Dtos.Navigations.Modification;

internal sealed record UpdateUserNavigationAppResponseDto
{
  public required NavigationId Id { get; init; }

  public required UserNavigationChangedFields ChangedNavigationFields { get; init; }

  public NavigationId? Parent { get; set; }

  public Icon? Icon { get; set; }

  public string? Title { get; set; }

  public bool? IsDeleted { get; set; }
}