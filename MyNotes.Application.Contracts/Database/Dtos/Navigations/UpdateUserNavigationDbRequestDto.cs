using MyNotes.Application.Contracts.Database.Enums.Navigations;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Dtos.Navigations;

internal sealed record UpdateUserNavigationDbRequestDto
{
  public required NavigationId Id { get; init; }

  public required UserNavigationUpdateFields NavigationUpdateField { get; init; }

  public NavigationId? Parent { get; set; }

  public bool? IsComposite { get; init; }

  public short? Icon { get; set; }

  public string? Title { get; set; }

  public bool? IsDeleted { get; set; }
}