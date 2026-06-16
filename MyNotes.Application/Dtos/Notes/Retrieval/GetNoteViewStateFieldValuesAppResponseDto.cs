using MyNotes.Application.Contracts.Database.Enums.Notes;

namespace MyNotes.Application.Dtos.Notes.Retrieval;

internal sealed record GetNoteViewStateFieldValuesAppResponseDto
{
  public required NoteViewStateGetFields GetFields { get; init; }
}