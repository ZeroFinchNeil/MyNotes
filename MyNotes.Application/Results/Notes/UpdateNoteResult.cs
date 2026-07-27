namespace MyNotes.Application.Results.Notes;

internal sealed record UpdateNoteResult
{
  public required AppUpdateStatus Status { get; init; }

  public DateTimeOffset? Modified { get; init; }
}