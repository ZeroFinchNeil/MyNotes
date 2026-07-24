using MyNotes.Application.Contracts.Navigations.Enums;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Navigations.Models.Retrieval;

internal sealed record GetNavigationFieldValuesDbRequestDto
{

  public required NavigationGetFields GetFields { get; init; }

  public required NavigationId Id { get; init; }
}
