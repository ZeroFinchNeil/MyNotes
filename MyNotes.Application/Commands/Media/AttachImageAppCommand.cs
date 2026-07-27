using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Commands.Media;

internal sealed record AttachImageAppCommand
{
  public required ImageId Id { get; init; }

  public required NoteId NoteId { get; init; }

  public required string FileNameWithExtension { get; init; }
}