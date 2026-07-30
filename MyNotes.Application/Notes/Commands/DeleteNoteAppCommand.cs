using MyNotes.Common.Enums.Modes;
using MyNotes.Domain.Notes;

namespace MyNotes.Application.Notes.Commands;

internal sealed record DeleteNoteAppCommand
{
  public required NoteId Id { get; init; }

  public required DeleteMode DeleteMode { get; init; }
}