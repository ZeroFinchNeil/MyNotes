using MyNotes.Application.Contracts.Enums.Navigations;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Models.Navigations.Retrieval;

internal sealed record GetNavigationFieldValuesDbRequestDto
{

  public required NavigationGetFields GetFields { get; init; }

  public required NavigationId Id { get; init; }
}
