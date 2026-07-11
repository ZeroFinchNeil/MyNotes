using MyNotes.Common.Enums.Modes;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Dtos.Navigations.Modification;

internal sealed record DeleteNavigationAppRequestDto
{
  public required NavigationId Id { get; init; }

  public required DeleteMode DeleteMode { get; init; }
}