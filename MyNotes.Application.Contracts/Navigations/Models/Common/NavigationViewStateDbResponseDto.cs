using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Navigations.Models.Common;

internal abstract record NavigationViewStateDbResponseDto
{
  public required NavigationId Id { get; init; }
}
