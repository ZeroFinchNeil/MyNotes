using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Models.Navigations.Modification;

internal abstract record UpdateNavigationViewStateDbRequestDto
{
  public required NavigationId Id { get; init; }
}