using System;

namespace MyNotes.Application.Contracts.Database.Dtos.Navigations.Common;

internal abstract record UserNavigationViewStateDbResponseDto
{
  public required Guid Id { get; init; }
}
