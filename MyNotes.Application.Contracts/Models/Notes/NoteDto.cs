using System;
using System.Collections.Generic;

using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Models.Notes;

internal sealed record NoteDto
{
  public required NoteId Id { get; init; }

  public required NavigationId NavigationId { get; init; }

  public required DateTimeOffset Created { get; init; }

  public required DateTimeOffset Modified { get; init; }

  public required string Title { get; init; }

  public required string Body { get; init; }

  public required IReadOnlyList<string> BodyImagePaths { get; init; }

  public required string BackgroundColor { get; init; }

  public required string? BackgroundImagePath { get; init; }

  public required bool IsBookmarked { get; init; }

  public required bool IsDeleted { get; init; }
}