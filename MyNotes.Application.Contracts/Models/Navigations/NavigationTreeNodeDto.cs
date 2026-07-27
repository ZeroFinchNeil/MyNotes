using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Models.Navigations;

internal abstract record NavigationTreeNodeDto
{
  public required NavigationId Id { get; init; }

  public required NavigationId ParentId { get; init; }

  public required int Icon { get; init; }

  public required string Title { get; init; }

  public required bool IsDeleted { get; init; }

  public required NavigationViewStateDto ViewStateDto { get; init; }
}