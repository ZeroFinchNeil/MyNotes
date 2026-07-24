using MyNotes.Common.Enums.Modes;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Navigations.Models.Modification;

internal sealed record DeleteNavigationDbRequestDto
{
  public required NavigationId Id { get; init; }

  public required DeleteMode DeleteMode { get; init; }
}