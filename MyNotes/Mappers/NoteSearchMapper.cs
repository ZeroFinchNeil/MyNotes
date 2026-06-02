using MyNotes.Infrastructure.Search.Documents.Notes;
using MyNotes.Models.Notes;

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
