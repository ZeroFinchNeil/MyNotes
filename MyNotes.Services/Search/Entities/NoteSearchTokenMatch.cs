using System;
using System.Collections.Immutable;

namespace MyNotes.Services.Search.Entities;

internal sealed class NoteSearchTokenMatch : IComparable<NoteSearchTokenMatch>
{
  public required float Score { get; init; }
  public required Guid NoteId { get; init; }
  public required int DocId { get; init; }

  public required int TitleMatchFrequency { get; init; }
  public required ImmutableList<Range> TitleMatchOffsets { get; init; }

  public required int BodyMatchFrequency { get; init; }
  public required ImmutableList<Range> BodyMatchOffsets { get; init; }

  public int CompareTo(NoteSearchTokenMatch? other) => other is not null ? other.Score.CompareTo(this.Score) : 1;
}