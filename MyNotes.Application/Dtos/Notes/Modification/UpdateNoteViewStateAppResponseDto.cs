using DotNext;

using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Dtos.Notes.Modification;

internal sealed record UpdateNoteViewStateAppResponseDto
{
  public required NoteId Id { get; init; }
}