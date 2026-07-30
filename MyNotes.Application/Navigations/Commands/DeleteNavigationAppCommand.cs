using MyNotes.Common.Enums.Modes;
using MyNotes.Domain.Navigations;

namespace MyNotes.Application.Navigations.Commands;

internal sealed record DeleteNavigationAppCommand
{
  public required NavigationId Id { get; init; }

  public required DeleteMode DeleteMode { get; init; }
}