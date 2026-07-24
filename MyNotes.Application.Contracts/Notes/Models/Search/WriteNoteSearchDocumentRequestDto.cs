using System;

namespace MyNotes.Application.Contracts.Notes.Models.Search;

internal class WriteNoteSearchDocumentRequestDto
{
  public required Guid Id { get; init; }

  public required string Title { get; set; }

  public required string Body { get; set; }
}
