using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Common.Mappers;
using MyNotes.Domain.Notes;

namespace MyNotes.Models.Notes;

internal class NoteModelFactory : IModelFactory<NoteDto, NoteModel>
{
  private readonly IModelStore<NoteId, NoteModel> Store;

  public NoteModelFactory(IModelStore<NoteId, NoteModel> store)
  {
    Store = store;
  }

  public NoteModel Create(NoteDto noteDto)
  {
    return Store.AddOrUpdate(noteDto.Id, _ => NoteMappers.ToModel(noteDto), (model) => NoteMappers.Apply(model, noteDto));
  }
}
