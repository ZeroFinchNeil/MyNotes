using MyNotes.Application.Contracts.Navigations.Models;

namespace MyNotes.Application.Navigations.Commands;

internal sealed record UpdateNavigationAppCommand
{
  public required NavigationPatchDto PatchDto { get; init; }
}