using MyNotes.Domain.ValueObjects;
using MyNotes.Templates;

namespace MyNotes.Application.Dtos.Navigations.Common;

internal abstract record NavigationAppResponseDto
{
  public required NavigationId Id { get; init; }

  public required NavigationId Parent { get; init; }

  public required Icon Icon { get; init; }

  public required string Title { get; init; }

  public required bool IsDeleted { get; init; }
}

/*
Id = ,
Parent = ,
Icon = ,
Title = ,
IsDeleted = ,
*/