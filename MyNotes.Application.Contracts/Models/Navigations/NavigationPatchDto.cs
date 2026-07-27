using DotNext;

using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Models.Navigations;

internal sealed record NavigationPatchDto
{
  public required NavigationId Id { get; init; }

  public Optional<NavigationId> ParentId { get; init; }

  public Optional<int> Icon { get; init; }

  public Optional<string> Title { get; init; }

  public Optional<bool> IsDeleted { get; init; }

  public bool IsEmpty => this is
  {
    ParentId.IsUndefined: true,
    Icon.IsUndefined: true,
    Title.IsUndefined: true,
    IsDeleted.IsUndefined: true
  };
}