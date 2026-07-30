using MyNotes.Application.Results;

namespace MyNotes.Application.Notes.Results;

internal sealed record UpdateNoteResult
{
  public required AppUpdateStatus Status { get; init; }

  public DateTimeOffset? Modified { get; init; }
}