using System;

using MyNotes.Application.Contracts.Database.Enums.Notes;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Dtos.Notes.Retrieval;

internal class GetNoteFieldValuesDbResponseDto
{
  public required NoteGetFields NoteGetFields { get; init; }

  public NoteId? Id { get; init; }

  public NavigationId? NavigationId { get; init; }

  public DateTimeOffset? Created { get; init; }

  public DateTimeOffset? Modified { get; init; }

  public string? Title { get; init; }

  public string? Body { get; init; }

  public string? BackgroundColor { get; init; }

  public bool? IsBookmarked { get; init; }

  public bool? IsDeleted { get; init; }
}
