using MyNotes.Common.Enums.Modes;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Dtos.Notes.Modification;

internal sealed record DeleteNoteAppRequestDto
{
  public required NoteId Id { get; init; }

  public required DeleteMode DeleteMode { get; init; }
}