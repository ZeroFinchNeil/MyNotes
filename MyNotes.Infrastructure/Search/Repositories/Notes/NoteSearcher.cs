using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MyNotes.Application.Contracts.Search.Dtos.Notes;
using MyNotes.Application.Contracts.Search.Repositories.Notes;
using MyNotes.Domain.Entities.Notes;
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
    NoteSearchDocument document = noteSearchDocumentDto.ToEntity();
    return await AppSearchContext.WriteNoteIndexAsync(document, cancellationToken);
  }

  public Task<IReadOnlyList<Note>> GetNotesAsync()
  {
    throw new System.NotImplementedException();
  }
}
