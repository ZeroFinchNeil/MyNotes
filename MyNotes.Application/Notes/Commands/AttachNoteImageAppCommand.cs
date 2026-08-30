using MyNotes.Domain.Notes;

namespace MyNotes.Application.Notes.Commands;

internal sealed record AttachNoteImageAppCommand
{
  public required NoteId NoteId { get; init; }

  public required string OriginalFilePath { get; init; }
}