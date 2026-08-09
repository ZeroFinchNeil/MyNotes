using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Notes.Commands;
using MyNotes.Application.Notes.Results;
using MyNotes.Application.Notes.Services;

namespace MyNotes.Services.Updates.Note;

internal sealed class NoteUpdateHandler(NoteService NoteService) : IUpdateHandler<NotePatchDto, UpdateNoteResult>
{
  public Task<UpdateNoteResult> HandleAsync(NotePatchDto patch, CancellationToken cancellationToken = default) => NoteService.Modification.UpdateNoteAsync(new UpdateNoteAppCommand() { PatchDto = patch }, cancellationToken);
}