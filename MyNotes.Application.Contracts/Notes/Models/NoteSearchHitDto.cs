using System;
using System.Collections.Generic;

using MyNotes.Domain.Notes;

namespace MyNotes.Application.Contracts.Notes.Models;

internal sealed record NoteSearchHitDto
{
  public required NoteId NoteId { get; init; }

  public required int TitleMatchFrequency { get; init; }
  public required IReadOnlyList<Range> TitleMatchRanges { get; init; }

  public required int BodyMatchFrequency { get; init; }
  public required IReadOnlyList<Range> BodyMatchRanges { get; init; }
}