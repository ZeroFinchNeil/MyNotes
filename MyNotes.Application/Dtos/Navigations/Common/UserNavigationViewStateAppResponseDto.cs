using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Dtos.Navigations.Common;

internal abstract record UserNavigationViewStateAppResponseDto
{
  public required NavigationId Id { get; init; }
}