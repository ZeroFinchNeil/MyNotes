using System.Collections.Generic;

namespace MyNotes.Infrastructure.Search.Documents.Notes;

internal sealed class NoteSearchResult
{
  public required string SearchText { get; init; }
  public required IAsyncEnumerable<NoteSearchTokenMatch> Matches { get; init; }
}