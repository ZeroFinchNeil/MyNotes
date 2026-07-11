using MyNotes.Application.Contracts.Database.Enums.Navigations;

namespace MyNotes.Application.Contracts.Database.Dtos.Navigations.Modification;

internal sealed record UpdateUserCompositeNavigationViewStateDbRequestDto : UpdateUserNavigationViewStateDbRequestDto
{
  public required UserCompositeNavigationViewStateUpdateFields UpdateFields { get; init; }

  public bool? IsExpanded { get; init; }
}