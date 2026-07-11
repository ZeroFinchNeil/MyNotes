using MyNotes.Application.Contracts.Database.Enums.Navigations;

namespace MyNotes.Application.Dtos.Navigations.Modification;

internal sealed record UpdateUserCompositeNavigationViewStateAppRequestDto : UpdateUserNavigationViewStateAppRequestDto
{
  public required UserCompositeNavigationViewStateUpdateFields UpdateFields { get; init; }

  public bool? IsExpanded { get; init; }
}