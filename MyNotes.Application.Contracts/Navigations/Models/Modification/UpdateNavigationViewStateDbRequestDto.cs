using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Navigations.Models.Modification;

internal abstract record UpdateNavigationViewStateDbRequestDto
{
  public required NavigationId Id { get; init; }
}