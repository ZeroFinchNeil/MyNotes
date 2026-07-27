using MyNotes.Application.Contracts.Models.Navigations;

namespace MyNotes.Application.Commands.Navigations;

internal sealed record UpdateNavigationAppCommand
{
  public required NavigationPatchDto PatchDto { get; init; }
}