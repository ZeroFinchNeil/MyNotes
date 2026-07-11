using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Dtos.Navigations.Common;

internal abstract record UserNavigationViewStateDbResponseDto
{
  public required NavigationId Id { get; init; }
}
