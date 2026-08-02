namespace MyNotes.Application.Media.Results;

internal sealed record MoveImageResult
{
  public required int SourceIndex { get; init; }

  public required int TargetIndex { get; init; }
}