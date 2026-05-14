using System;

using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Dtos.Notes;

internal record NoteDbResponseDto
{
  public required NoteId Id { get; init; }

  public required NavigationId NavigationId { get; init; }

  public required DateTimeOffset Created { get; init; }

  public DateTimeOffset Modified { get; init; }

  public required string Title { get; init; }

  public required string Body { get; init; }

  public required string BackgroundColor { get; init; }

  public required bool IsBookmarked { get; init; }

  public required bool IsDeleted { get; init; }
}
