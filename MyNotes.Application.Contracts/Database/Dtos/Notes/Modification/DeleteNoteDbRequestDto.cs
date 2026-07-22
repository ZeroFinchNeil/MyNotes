using MyNotes.Common.Enums.Modes;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Dtos.Notes.Modification;

internal sealed record DeleteNoteDbRequestDto
{
  public required NoteId Id { get; init; }

  public required DeleteMode DeleteMode { get; init; }
}