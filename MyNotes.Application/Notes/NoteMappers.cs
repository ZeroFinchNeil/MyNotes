using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Debugging.Attributes;
using MyNotes.Domain.Notes;

namespace MyNotes.Application.Notes;

[AssemblyLocal]
internal static class NoteMappers
{
  public static NoteDto ToDto(Note note, NoteViewStateDto viewStateDto) => new()
  {
    Id = note.Id,
    NavigationId = note.NavigationId,
    Created = note.Created,
    Modified = note.Modified,
    Title = note.Title,
    Body = note.Body,
    BackgroundColor = note.BackgroundColor,
    BackgroundImagePath = note.BackgroundImagePath,
    IsBookmarked = note.IsBookmarked,
    IsDeleted = note.IsDeleted,
    ViewStateDto = viewStateDto
  };

  public static NoteSearchDocumentDto ToSearchDocumentDto(NoteId noteId, string title, string bodyPlainText) => new()
  {
    Id = noteId.Value,
    Title = title,
    Body = bodyPlainText
  };
}