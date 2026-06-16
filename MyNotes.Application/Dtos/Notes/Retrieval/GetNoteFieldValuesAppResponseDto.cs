using MyNotes.Application.Contracts.Database.Enums.Notes;

namespace MyNotes.Application.Dtos.Notes.Retrieval;

internal sealed record GetNoteFieldValuesAppResponseDto
{
  public required NoteGetFields GetFields { get; init; }
}