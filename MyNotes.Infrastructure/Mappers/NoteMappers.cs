using System;

using MyNotes.Application.Contracts.Database.Dtos.Notes.Common;
using MyNotes.Application.Contracts.Database.Dtos.Notes.Creation;
using MyNotes.Application.Contracts.Search.Dtos.Notes;
using MyNotes.Debugging.Attributes;
using MyNotes.Infrastructure.Database.Entities.Notes;
using MyNotes.Infrastructure.Search.Documents.Notes;

namespace MyNotes.Infrastructure.Mappers;

[AssemblyLocal]
internal static class NoteMappers
{
  public static void Test() { }
  public static NoteEntity ToEntity(CreateNoteDbRequestDto noteDbDto) => throw new NotImplementedException();
  public static NoteViewStateEntity ToEntity(CreateNoteViewStateDbRequestDto noteDbDto) => throw new NotImplementedException();

  public static NoteBundleDbResponseDto ToDto(NoteEntity noteEntity, NoteViewStateEntity noteViewStateEntity) => throw new NotImplementedException();

  public static CreateNoteDbRequestDto ToDto(NoteEntity noteDbDto) => throw new NotImplementedException();
  public static NoteViewStateDbResponseDto ToDto(NoteViewStateEntity noteDbDto) => throw new NotImplementedException();

  public static NoteSearchDocument ToEntity(NoteSearchDocumentDto noteSearchDocumentDto) => throw new NotImplementedException();
}

internal static class NoteMappingExtensions
{
  extension(NoteSearchDocumentDto dto)
  {
    public NoteSearchDocument ToEntity() => NoteMappers.ToEntity(dto);
  }
}
