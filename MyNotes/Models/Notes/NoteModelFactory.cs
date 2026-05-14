using MyNotes.Application.Contracts.Database.Repositories.Notes;
using MyNotes.Application.Dtos.Notes;
using MyNotes.Domain.ValueObjects;
using MyNotes.Mappers;

namespace MyNotes.Models.Notes;

internal class NoteModelFactory : IModelFactory<NoteAppResponseDto, NoteModel>
{
  public NoteModelFactory(INoteRepository noteRepository)
  {
    NoteRepository = noteRepository;
  }

  public NoteModel Create(NoteAppResponseDto noteDto)
  {
    return NoteMappers.ToModel(noteDto);
  }

  public NoteModel CreateDefault(NoteId noteId) => new()
  {

  };
}
