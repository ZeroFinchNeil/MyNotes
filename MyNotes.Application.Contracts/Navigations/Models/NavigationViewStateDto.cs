using MyNotes.Domain.Navigations;

namespace MyNotes.Application.Contracts.Navigations.Models;

internal abstract record NavigationViewStateDto
{
  public required NavigationId Id { get; init; }
}