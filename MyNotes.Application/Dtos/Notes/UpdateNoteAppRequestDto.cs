using MyNotes.Application.Contracts.Database.Enums.Notes;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Dtos.Notes;

internal sealed record UpdateNoteAppRequestDto
{
  public required NoteId Id { get; init; }

  public required NoteUpdateFields NoteUpdateField { get; init; }

  public NavigationId? NavigationId { get; init; }

  public DateTimeOffset? Created { get; init; }

  public DateTimeOffset? Modified { get; init; }

  public string? Title { get; init; }

  public string? Body { get; init; }

  public string? BodyPlainText { get; init; }

  public bool? IsBookmarked { get; init; }

  public bool? IsDeleted { get; init; }
}

/*
UpdateNoteAppRequestDto dto = new()
{
  Id = ,
  NoteUpdateField = ,
};
*/