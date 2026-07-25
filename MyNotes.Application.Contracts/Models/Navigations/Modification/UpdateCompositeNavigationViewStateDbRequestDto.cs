using MyNotes.Application.Contracts.Enums.Navigations;

namespace MyNotes.Application.Contracts.Models.Navigations.Modification;

internal sealed record UpdateCompositeNavigationViewStateDbRequestDto : UpdateNavigationViewStateDbRequestDto
{
  public required CompositeNavigationViewStateUpdateFields UpdateFields { get; init; }

  public bool? IsExpanded { get; init; }
}