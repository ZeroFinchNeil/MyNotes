using System.Collections.Generic;
using System.Threading.Tasks;

using MyNotes.Application.Contracts.Search.Dtos.Notes;
using MyNotes.Application.Contracts.Search.Repositories.Notes;
using MyNotes.Domain.Entities.Notes;
using MyNotes.Infrastructure.Search.Core;

namespace MyNotes.Infrastructure.Search.Repositories.Notes;

internal class NoteSearcher : INoteSearcher
{
  private readonly AppSearchContext AppSearchContext;

  public NoteSearcher(AppSearchContext appSearchContext)
  {
    AppSearchContext = appSearchContext;
  }

  public Task<bool> WriteNoteIndexAsync(NoteSearchDocumentDto noteSearchDocumentDto)
  {
    throw new System.NotImplementedException();
  }

  public Task<IReadOnlyList<Note>> GetNotesAsync()
  {
    throw new System.NotImplementedException();
  }
}
