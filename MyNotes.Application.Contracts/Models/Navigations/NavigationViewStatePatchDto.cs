using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Models.Navigations;

internal abstract record NavigationViewStatePatchDto
{
  public required NavigationId Id { get; init; }

  public abstract bool IsEmpty { get; }
}