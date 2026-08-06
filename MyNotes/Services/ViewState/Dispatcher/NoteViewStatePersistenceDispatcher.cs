using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Notes.Services;

namespace MyNotes.Services.ViewState.Dispatcher;

internal sealed partial class NoteViewStatePersistenceDispatcher(NoteService NoteService) : ViewStatePersistenceDispatcher<NoteViewStatePatchDto>
{
  protected override Task WriteAsync(NoteViewStatePatchDto patchDto, CancellationToken cancellationToken = default) =>
    NoteService.Modification.UpdateNoteViewStateAsync(new Application.Notes.Commands.UpdateNoteViewStateAppCommand() { PatchDto = patchDto }, cancellationToken);
}