namespace MyNotes.Application.Notes.Results;

internal sealed record MoveNoteImageResult
{
  public required int SourceIndex { get; init; }

  public required int TargetIndex { get; init; }
}