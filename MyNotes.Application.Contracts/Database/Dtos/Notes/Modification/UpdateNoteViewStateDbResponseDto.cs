using System;

using DotNext;

using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Dtos.Notes.Modification;

internal sealed record UpdateNoteViewStateDbResponseDto
{
  public required NoteId Id { get; init; }
}