using MyNotes.Application.Contracts.Notes.Models;

namespace MyNotes.Application.Notes.Commands;

internal sealed record UpdateNoteAppCommand
{
  public required NotePatchDto PatchDto { get; init; }
}