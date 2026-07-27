using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Models.Navigations;

internal abstract record NavigationViewStateDto
{
  public required NavigationId Id { get; init; }
}