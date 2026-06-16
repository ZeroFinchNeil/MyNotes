using System;

using MyNotes.Application.Contracts.Database.Dtos.Notes.Creation;
using MyNotes.Infrastructure.Database.Entities.Notes;

namespace MyNotes.Infrastructure.Mappers;

internal static class NoteMappers
{
  public static NoteEntity ToEntity(CreateNoteDbRequestDto noteDbDto) => throw new NotImplementedException();
  public static NoteViewStateEntity ToEntity(CreateNoteViewStateDbRequestDto noteDbDto) => throw new NotImplementedException();
  public static CreateNoteDbRequestDto ToDto(NoteEntity noteDbDto) => throw new NotImplementedException();
  public static CreateNoteViewStateDbRequestDto ToDto(NoteViewStateEntity noteDbDto) => throw new NotImplementedException();

}
