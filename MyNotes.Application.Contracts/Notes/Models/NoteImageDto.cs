using MyNotes.Domain.Media;
using MyNotes.Domain.Notes;

namespace MyNotes.Application.Contracts.Notes.Models;

internal sealed record NoteImageDto
{
  public required ImageId Id { get; init; }

  public required NoteId NoteId { get; init; }

  public required string OriginalFileName { get; init; }

  public required string StoredExtension { get; init; }
}