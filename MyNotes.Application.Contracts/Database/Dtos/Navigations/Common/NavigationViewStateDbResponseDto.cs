using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Dtos.Navigations.Common;

internal abstract record NavigationViewStateDbResponseDto
{
  public required NavigationId Id { get; init; }
}
