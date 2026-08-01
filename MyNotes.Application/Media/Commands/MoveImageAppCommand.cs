using MyNotes.Domain.Media;

namespace MyNotes.Application.Media.Commands;

internal sealed record MoveImageAppCommand
{
  public required ImageId SourceId { get; init; }

  public required ImageId TargetId { get; init; }
}