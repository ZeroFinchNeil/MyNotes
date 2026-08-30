using MyNotes.Domain.Media;

namespace MyNotes.Application.Notes.Commands;

internal sealed record MoveNoteImageAppCommand
{
  public required ImageId SourceId { get; init; }

  public required ImageId TargetId { get; init; }
}