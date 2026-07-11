using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Dtos.Navigations.Modification;

internal abstract record UpdateUserNavigationViewStateAppRequestDto
{
  public required NavigationId Id { get; init; }
}