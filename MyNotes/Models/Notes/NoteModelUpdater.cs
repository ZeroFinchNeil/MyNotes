using MyNotes.Application.Dtos.Notes.Common;

namespace MyNotes.Models.Notes;

internal sealed class NoteModelUpdater : IModelUpdater<NoteBundleAppResponseDto, NoteModel>
{
  public void Update(NoteModel target, NoteBundleAppResponseDto source)
  {

  }
}