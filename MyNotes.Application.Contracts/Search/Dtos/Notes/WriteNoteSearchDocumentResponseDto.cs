using System;

namespace MyNotes.Application.Contracts.Search.Dtos.Notes;

internal sealed record WriteNoteSearchDocumentResponseDto
{
  public required Guid Id { get; init; }

  public required string Title { get; set; }

  public required string Body { get; set; }
}