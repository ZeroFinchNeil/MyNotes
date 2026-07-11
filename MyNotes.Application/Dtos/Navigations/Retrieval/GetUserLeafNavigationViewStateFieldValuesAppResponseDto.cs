using MyNotes.Application.Contracts.Database.Enums.Navigations;

namespace MyNotes.Application.Dtos.Navigations.Retrieval;

internal sealed record GetUserLeafNavigationViewStateFieldValuesAppResponseDto
{
  public required UserCompositeNavigationViewStateGetFields GetFields { get; init; }
}