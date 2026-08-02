using MyNotes.Domain.Media;
using MyNotes.Domain.Notes;

namespace MyNotes.Application.Contracts.Media.Models;

internal sealed record ImageDto
{
  public required ImageId Id { get; init; }

  public required NoteId NoteId { get; init; }

  public required string OriginalFileName { get; init; }

  public required string StoredExtension { get; init; }
}