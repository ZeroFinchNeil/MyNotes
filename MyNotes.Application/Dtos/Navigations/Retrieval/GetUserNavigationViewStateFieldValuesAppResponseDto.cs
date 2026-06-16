using MyNotes.Application.Contracts.Database.Enums.Navigations;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Dtos.Navigations.Retrieval;

internal sealed record GetUserNavigationViewStateFieldValuesAppResponseDto
{
  public required UserNavigationViewStateGetFields GetFields { get; init; }
}