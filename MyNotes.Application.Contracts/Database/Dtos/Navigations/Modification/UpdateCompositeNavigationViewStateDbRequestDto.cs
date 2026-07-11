using MyNotes.Application.Contracts.Database.Enums.Navigations;

namespace MyNotes.Application.Contracts.Database.Dtos.Navigations.Modification;

internal sealed record UpdateCompositeNavigationViewStateDbRequestDto : UpdateNavigationViewStateDbRequestDto
{
  public required CompositeNavigationViewStateUpdateFields UpdateFields { get; init; }

  public bool? IsExpanded { get; init; }
}