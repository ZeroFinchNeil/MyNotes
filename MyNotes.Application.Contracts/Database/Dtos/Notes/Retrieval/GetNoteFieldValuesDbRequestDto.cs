using System;

using MyNotes.Application.Contracts.Database.Enums.Notes;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Dtos.Notes.Retrieval;

internal class GetNoteFieldValuesDbRequestDto
{
  public required NoteGetFields GetFields { get; init; }

  public NoteId Id { get; init; }
}
