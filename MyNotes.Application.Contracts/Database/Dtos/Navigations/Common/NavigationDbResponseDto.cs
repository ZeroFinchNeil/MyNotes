using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Dtos.Navigations.Common;

internal sealed record NavigationDbResponseDto
{
  public required NavigationId Id { get; init; }

  public required NavigationId Parent { get; init; }

  public required bool IsComposite { get; init; }

  public required int Icon { get; init; }

  public required string Title { get; init; }

  public required int Position { get; init; }

  public required bool IsDeleted { get; init; }
}
