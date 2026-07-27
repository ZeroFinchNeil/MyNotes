using MyNotes.Application.Contracts.Models.Navigations;

namespace MyNotes.Application.Commands.Navigations;

internal sealed record UpdateNavigationViewStateAppCommand
{
  public required NavigationViewStatePatchDto PatchDto { get; init; }
}