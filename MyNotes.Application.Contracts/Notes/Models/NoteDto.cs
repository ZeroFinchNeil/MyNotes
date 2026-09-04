using System;

using MyNotes.Domain.Navigations;
using MyNotes.Domain.Notes;

namespace MyNotes.Application.Contracts.Notes.Models;

internal sealed record NoteDto
{
  public required NoteId Id { get; init; }

  public required NavigationId NavigationId { get; init; }

  public required DateTimeOffset Created { get; init; }

  public required DateTimeOffset Modified { get; init; }

  public required string Title { get; init; }

  public required byte[] Body { get; init; }

  public required string BackgroundColor { get; init; }

  public required string? BackgroundImagePath { get; init; }

  public required bool IsBookmarked { get; init; }

  public required bool IsDeleted { get; init; }

  public required NoteViewStateDto ViewStateDto { get; init; }
}