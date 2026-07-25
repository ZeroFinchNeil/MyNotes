using MyNotes.Application.Contracts.Models.Notes;

namespace MyNotes.Application.Commands.Notes;

internal sealed record UpdateNoteAppCommand
{
  public required NotePatchDto NotePatchDto { get; init; }

  public required DateTimeOffset Modified { get; init; }
}