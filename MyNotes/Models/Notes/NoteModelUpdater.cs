using MyNotes.Application.Contracts.Notes.Models;

namespace MyNotes.Models.Notes;

internal sealed class NoteModelUpdater : IModelUpdater<NoteDto, NoteModel>
{
  public void Update(NoteModel target, NoteDto source)
  {

  }
}