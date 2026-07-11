using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Dtos.Navigations.Modification;

internal abstract record UpdateUserNavigationViewStateDbRequestDto
{
  public required NavigationId Id { get; init; }
}