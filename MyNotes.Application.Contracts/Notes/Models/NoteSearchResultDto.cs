namespace MyNotes.Application.Contracts.Notes.Models;

internal sealed class NoteSearchResultDto
{
  public required NoteDto NoteDto { get; init; }

  public required NoteSearchHitDto HitDto { get; init; }
}