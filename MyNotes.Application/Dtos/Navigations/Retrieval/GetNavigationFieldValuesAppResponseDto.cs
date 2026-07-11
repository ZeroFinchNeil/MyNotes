using MyNotes.Application.Contracts.Database.Enums.Navigations;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Dtos.Navigations.Retrieval;

internal sealed record GetNavigationFieldValuesAppResponseDto
{
  public required NavigationGetFields GetFields { get; init; }

  public NavigationId? Id { get; init; }

  public NavigationId? Parent { get; init; }

  public bool? IsComposite { get; init; }

  public int? Icon { get; init; }

  public string? Title { get; init; }

  public int? Position { get; init; }
}