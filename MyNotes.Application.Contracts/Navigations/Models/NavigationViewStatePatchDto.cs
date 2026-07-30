using MyNotes.Domain.Navigations;

namespace MyNotes.Application.Contracts.Navigations.Models;

internal abstract record NavigationViewStatePatchDto
{
  public required NavigationId Id { get; init; }

  public abstract bool IsEmpty { get; }
}