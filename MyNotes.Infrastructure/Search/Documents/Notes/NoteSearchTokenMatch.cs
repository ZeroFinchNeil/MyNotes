using System;
using System.Collections.Immutable;

namespace MyNotes.Infrastructure.Search.Documents.Notes;

internal sealed class NoteSearchTokenMatch : IComparable<NoteSearchTokenMatch>
{
  public required float Score { get; init; }
  public required Guid NoteId { get; init; }
  public required int DocId { get; init; }

  public required int TitleMatchFrequency { get; init; }
  public required ImmutableList<Range> TitleMatchRanges { get; init; }

  public required int BodyMatchFrequency { get; init; }
  public required ImmutableList<Range> BodyMatchRanges { get; init; }

  public int CompareTo(NoteSearchTokenMatch? other) => other is not null ? other.Score.CompareTo(this.Score) : 1;
}