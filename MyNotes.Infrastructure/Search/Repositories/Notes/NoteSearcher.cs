using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Notes.Persistence;
using MyNotes.Domain.Notes;
using MyNotes.Infrastructure.Mappers;
using MyNotes.Infrastructure.Search.Core;
using MyNotes.Infrastructure.Search.Documents.Notes;

namespace MyNotes.Infrastructure.Search.Repositories.Notes;

internal class NoteSearcher : INoteSearcher
{
  private readonly AppSearchContext AppSearchContext;

  public NoteSearcher(AppSearchContext appSearchContext)
  {
    AppSearchContext = appSearchContext;
  }

  public async Task<bool> WriteNoteIndexAsync(NoteSearchDocumentDto noteSearchDocumentDto, CancellationToken cancellationToken = default)
  {
    NoteSearchDocument document = NoteMappers.ToEntity(noteSearchDocumentDto);
    return await AppSearchContext.WriteNoteIndexAsync(document, cancellationToken);
  }

  public Task<IReadOnlyList<Note>> GetNotesAsync(CancellationToken cancellationToken = default)
  {
    throw new System.NotImplementedException();
  }

  public Task DeleteNoteIndexAsync(NoteId noteId, CancellationToken cancellationToken = default) => AppSearchContext.DeleteNoteIndexAsync(noteId.Value, cancellationToken);

  public Task CommitAsync(CancellationToken cancellationToken = default) => AppSearchContext.CommitAsync(cancellationToken);
}
