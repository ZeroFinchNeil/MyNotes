using MyNotes.Application.Contracts.Database.Enums.Navigations;

namespace MyNotes.Application.Dtos.Navigations.Retrieval;

internal sealed record GetUserNavigationViewStateFieldValuesAppResponseDto
{
  public required UserNavigationViewStateGetFields GetFields { get; init; }
}