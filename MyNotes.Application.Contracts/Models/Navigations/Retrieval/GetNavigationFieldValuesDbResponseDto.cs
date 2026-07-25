using MyNotes.Application.Contracts.Enums.Navigations;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Models.Navigations.Retrieval;

internal sealed record GetNavigationFieldValuesDbResponseDto
{
  public required NavigationGetFields GetFields { get; init; }

  public NavigationId? Id { get; init; }

  public NavigationId? Parent { get; init; }

  public bool? IsComposite { get; init; }

  public int? Icon { get; init; }

  public string? Title { get; init; }

  public bool? IsDeleted { get; init; }
}
