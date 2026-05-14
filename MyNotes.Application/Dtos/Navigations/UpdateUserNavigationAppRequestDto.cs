using MyNotes.Application.Contracts.Database.Enums.Navigations;
using MyNotes.Domain.ValueObjects;
using MyNotes.Templates;

namespace MyNotes.Application.Dtos.Navigations;

internal sealed record UpdateUserNavigationAppRequestDto
{
  public required NavigationId Id { get; init; }

  public required UserNavigationUpdateFields NavigationUpdateField { get; init; }

  public NavigationId? Parent { get; set; }

  public bool? IsComposite { get; init; }

  public Icon? Icon { get; set; }

  public string? Title { get; set; }

  public bool? IsDeleted { get; set; }
}