using MyNotes.Common.Enums.Modes;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Commands.Notes;

internal sealed record DeleteNoteAppCommand
{
  public required NoteId Id { get; init; }

  public required DeleteMode DeleteMode { get; init; }
}