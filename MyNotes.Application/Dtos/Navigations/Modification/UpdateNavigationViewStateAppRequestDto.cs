using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Dtos.Navigations.Modification;

internal abstract record UpdateNavigationViewStateAppRequestDto
{
  public required NavigationId Id { get; init; }
}