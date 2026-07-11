using MyNotes.Application.Contracts.Database.Enums.Navigations;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Dtos.Navigations.Retrieval;

internal sealed record GetNavigationFieldValuesDbRequestDto
{

  public required NavigationGetFields GetFields { get; init; }

  public required NavigationId Id { get; init; }
}
