using System;
using System.Collections.Generic;

using MyNotes.Domain.ValueObjects;

namespace MyNotes.Domain.Entities.Notes;

internal sealed class Note
{
  public required NoteId Id { get; init; }

  public required NavigationId NavigationId { get; set; }

  public required DateTimeOffset Created { get; init; }

  public DateTimeOffset Modified { get; set; }

  public required string Title { get; set; }

  public required string Body { get; set; }

  public required IReadOnlyList<string> BodyImagePaths { get; set; } 

  public required string BackgroundColor { get; set; }

  public required string? BackgroundImagePath { get; set; }

  public required bool IsBookmarked { get; set; }

  public required bool IsDeleted { get; set; }
}