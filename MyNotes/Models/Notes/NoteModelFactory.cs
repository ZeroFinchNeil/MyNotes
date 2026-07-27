using MyNotes.Application.Contracts.Models.Notes;
using MyNotes.Domain.ValueObjects;
using MyNotes.Mappers;

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
