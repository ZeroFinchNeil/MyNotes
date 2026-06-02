using MyNotes.Application.Contracts.Database.Enums.Navigations;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Dtos.Navigations.Retrieval;

internal sealed record GetUserNavigationFieldValuesDbRequestDto
{

  public required UserNavigationGetFields UserNavigationGetFields { get; init; }

  public required NavigationId Id { get; init; }
}
