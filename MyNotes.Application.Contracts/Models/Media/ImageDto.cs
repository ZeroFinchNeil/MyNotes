using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Models.Media;

internal sealed record ImageDto
{
  public required ImageId Id { get; init; }

  public required NoteId NoteId { get; init; }

  public required string FileNameWithExtension { get; init; }
}