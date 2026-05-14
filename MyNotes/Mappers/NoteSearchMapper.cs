using MyNotes.Models.Notes;
using MyNotes.Services.Search.Entities;

namespace MyNotes.Mappers;

internal static class NoteSearchMapper
{
  public static NoteSearchDocument NoteToSearchDocument(NoteModel note) => new()
  {
    Id = note.Id.Value,
    Title = note.Title,
    Body = note.BodyPlainText
  };

  extension(NoteModel note)
  {
    public NoteSearchDocument ToSearchDocument() => NoteToSearchDocument(note);
  }
}
