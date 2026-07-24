using MyNotes.Application.Contracts.Navigations.Enums;

namespace MyNotes.Application.Contracts.Navigations.Models.Modification;

internal sealed record UpdateCompositeNavigationViewStateDbRequestDto : UpdateNavigationViewStateDbRequestDto
{
  public required CompositeNavigationViewStateUpdateFields UpdateFields { get; init; }

  public bool? IsExpanded { get; init; }
}