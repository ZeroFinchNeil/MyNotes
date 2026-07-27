using MyNotes.Application.Contracts.Models.Notes;

namespace MyNotes.Application.Commands.Notes;

internal sealed record UpdateNoteViewStateAppCommand
{
  public required NoteViewStatePatchDto PatchDto { get; init; }
}