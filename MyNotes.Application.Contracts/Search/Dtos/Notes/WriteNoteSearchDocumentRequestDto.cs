using System;

namespace MyNotes.Application.Contracts.Search.Dtos.Notes;

internal class WriteNoteSearchDocumentRequestDto
{
  public required Guid Id { get; init; }

  public required string Title { get; set; }

  public required string Body { get; set; }
}
