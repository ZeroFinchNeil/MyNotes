using MyNotes.Common.Enums.Modes;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Commands.Navigations;

internal sealed record DeleteNavigationAppCommand
{
  public required NavigationId Id { get; init; }

  public required DeleteMode DeleteMode { get; init; }
}