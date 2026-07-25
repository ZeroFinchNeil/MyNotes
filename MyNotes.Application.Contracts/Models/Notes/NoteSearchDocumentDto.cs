using System;

namespace MyNotes.Application.Contracts.Models.Notes;

internal sealed record NoteSearchDocumentDto
{
  public required Guid Id { get; init; }

  public required string Title { get; init; }

  public required string Body { get; init; }
}