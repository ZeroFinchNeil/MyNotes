using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MyNotes.Application.Contracts.Models.Notes;
using MyNotes.Domain.Entities.Notes;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Persistence.Notes;

internal interface INoteSearcher
{
  public Task<bool> WriteNoteIndexAsync(NoteSearchDocumentDto noteSearchDocumentDto, CancellationToken cancellationToken = default);

  public Task<IReadOnlyList<Note>> GetNotesAsync(CancellationToken cancellationToken = default);

  public Task CommitAsync(CancellationToken cancellationToken = default);

  public Task DeleteNoteIndexAsync(NoteId noteId, CancellationToken cancellationToken = default);
}
