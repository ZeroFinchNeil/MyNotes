using System.Collections.Generic;

namespace MyNotes.Services.Search.Entities;

internal sealed class NoteSearchResult
{
  public required string SearchText { get; init; }
  public required IAsyncEnumerable<NoteSearchTokenMatch> Matches { get; init; }
}