using MyNotes.Application.Contracts.Navigations.Models;

namespace MyNotes.Application.Navigations.Commands;

internal sealed record UpdateNavigationViewStateAppCommand
{
  public required NavigationViewStatePatchDto PatchDto { get; init; }
}