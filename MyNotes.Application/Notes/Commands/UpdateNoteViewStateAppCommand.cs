using MyNotes.Application.Contracts.Notes.Models;

namespace MyNotes.Application.Notes.Commands;

internal sealed record UpdateNoteViewStateAppCommand
{
  public required NoteViewStatePatchDto PatchDto { get; init; }
}