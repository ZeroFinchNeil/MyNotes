using MyNotes.Application.Contracts.Database.Enums.Navigations;

namespace MyNotes.Application.Dtos.Navigations.Retrieval;

internal sealed record GetLeafNavigationViewStateFieldValuesAppResponseDto
{
  public required CompositeNavigationViewStateGetFields GetFields { get; init; }
}