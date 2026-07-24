using MyNotes.Application.Dtos.Notes.Common;
using MyNotes.Domain.ValueObjects;
using MyNotes.Mappers;

namespace MyNotes.Models.Notes;

internal class NoteModelFactory : IModelFactory<NoteBundleAppResponseDto, NoteModel>
{
  private readonly IModelStore<NoteId, NoteModel> Store;

  public NoteModelFactory(IModelStore<NoteId, NoteModel> store)
  {
    Store = store;
  }

  public NoteModel Create(NoteBundleAppResponseDto noteBundleAppResponseDto)
  {
    return Store.AddOrUpdate(noteBundleAppResponseDto.Id, _ => noteBundleAppResponseDto.ToModel(), (model) => model.Apply(noteBundleAppResponseDto));
  }
}
