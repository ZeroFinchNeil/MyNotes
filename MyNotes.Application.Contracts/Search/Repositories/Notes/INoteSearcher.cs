using System.Collections.Generic;
using System.Threading.Tasks;

using MyNotes.Application.Contracts.Search.Dtos.Notes;
using MyNotes.Domain.Entities.Notes;

namespace MyNotes.Application.Contracts.Search.Repositories.Notes;

internal interface INoteSearcher
{
  public Task<bool> WriteNoteIndexAsync(NoteSearchDocumentDto noteSearchDocumentDto);

  public Task<IReadOnlyList<Note>> GetNotesAsync();
}
