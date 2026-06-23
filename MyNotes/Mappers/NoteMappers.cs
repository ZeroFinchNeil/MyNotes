using MyNotes.Application.Dtos.Notes.Common;
using MyNotes.Domain.Entities.Notes;
using MyNotes.Models.Notes;

namespace MyNotes.Mappers;

internal static class NoteMappers
{
  public static NoteModel ToModel(NoteBundleAppResponseDto noteDto) => throw new NotImplementedException();

  public static Note ToDomain(NoteModel noteModel) => throw new NotImplementedException();

  public static void Apply(NoteModel noteModel, NoteBundleAppResponseDto noteDto) => throw new NotImplementedException();
}

internal static class NoteMappingExtensions
{
  extension(NoteModel model)
  {
    public void Apply(NoteBundleAppResponseDto noteDto) => NoteMappers.Apply(model, noteDto);
  }

  extension(NoteBundleAppResponseDto dto)
  {
    public NoteModel ToModel() => NoteMappers.ToModel(dto);
  }
}