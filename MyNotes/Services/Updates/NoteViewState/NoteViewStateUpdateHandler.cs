using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Notes.Services;

namespace MyNotes.Services.Updates.NoteViewState;

internal sealed class NoteViewStateUpdateHandler(NoteService NoteService) : IUpdateHandler<NoteViewStatePatchDto>
{
  public Task HandleAsync(NoteViewStatePatchDto patch, CancellationToken cancellationToken = default) => NoteService.Modification.UpdateNoteViewStateAsync(new Application.Notes.Commands.UpdateNoteViewStateAppCommand() { PatchDto = patch }, cancellationToken);
}