using MyNotes.Application.Contracts.Database.Enums.Navigations;

namespace MyNotes.Application.Dtos.Navigations.Modification;

internal sealed record UpdateCompositeNavigationViewStateAppRequestDto : UpdateNavigationViewStateAppRequestDto
{
  public required CompositeNavigationViewStateUpdateFields UpdateFields { get; init; }

  public bool? IsExpanded { get; init; }
}