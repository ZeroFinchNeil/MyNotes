using MyNotes.Application.Contracts.Models.Notes;

namespace MyNotes.Models.Notes;

internal sealed class NoteModelUpdater : IModelUpdater<NoteDto, NoteModel>
{
  public void Update(NoteModel target, NoteDto source)
  {

  }
}