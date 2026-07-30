using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Domain.Notes;

namespace MyNotes.Application.Contracts.Notes.Persistence;

internal interface INoteSearcher
{
  public Task<bool> WriteNoteIndexAsync(NoteSearchDocumentDto noteSearchDocumentDto, CancellationToken cancellationToken = default);

  public Task<IReadOnlyList<Note>> GetNotesAsync(CancellationToken cancellationToken = default);

  public Task CommitAsync(CancellationToken cancellationToken = default);

  public Task DeleteNoteIndexAsync(NoteId noteId, CancellationToken cancellationToken = default);
}
