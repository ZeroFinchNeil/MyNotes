using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Dtos.Navigations.Creation;

internal sealed record CreateUserNavigationDbRequestDto
{
  public required NavigationId Id { get; init; }

  public required NavigationId InsertTargetId { get; init; }

  public required NavigationInsertPosition NavigationInsertPosition { get; init; }

  public required bool IsComposite { get; init; }

  public required int Icon { get; init; }

  public required string Title { get; init; }
}

/*
Id = ,
TargetId = ,
NavigationInsertPosition = ,
IsComposite = ,
Icon = ,
Title = ,
*/