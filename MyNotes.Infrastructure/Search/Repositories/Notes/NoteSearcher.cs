using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

  public async IAsyncEnumerable<NoteSearchHitDto> GetNotesAsync(string searchText, [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    var searchResult = await AppSearchContext.SearchNoteIndexAsync(searchText, cancellationToken);
    if (searchResult is null)
    {
      yield break;
    }

    await foreach(var match in searchResult.Matches)
    {
      yield return new NoteSearchHitDto()
      {
        NoteId = NoteId.Create(match.NoteId),
        TitleMatchFrequency = match.TitleMatchFrequency,
        TitleMatchRanges = match.TitleMatchRanges,
        BodyMatchFrequency = match.BodyMatchFrequency,
        BodyMatchRanges = match.BodyMatchRanges
      };
    }
  }

  public Task DeleteNoteIndexAsync(NoteId noteId, CancellationToken cancellationToken = default) => AppSearchContext.DeleteNoteIndexAsync(noteId.Value, cancellationToken);

  public Task CommitAsync(CancellationToken cancellationToken = default) => AppSearchContext.CommitAsync(cancellationToken);
}
