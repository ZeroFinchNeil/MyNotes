using MyNotes.Application.Contracts.Database.Dtos.Notes.Creation;
using MyNotes.Application.Contracts.Database.Repositories.Notes;
using MyNotes.Application.Contracts.Search.Dtos.Notes;
using MyNotes.Application.Contracts.Search.Repositories.Notes;
using MyNotes.Application.Dtos.Notes.Common;
using MyNotes.Application.Mappers;
using MyNotes.Domain.Entities.Notes;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Services.Notes;

internal sealed partial class NoteCreationService
{
  private readonly INoteRepository NoteRepository;
  private readonly NoteFactory NoteFactory;
  private readonly INoteSearcher NoteSearcher;

  public NoteCreationService(INoteRepository noteRepository, NoteFactory noteFactory, INoteSearcher noteSearcher)
  {
    NoteRepository = noteRepository;
    NoteFactory = noteFactory;
    NoteSearcher = noteSearcher;
  }

  public async Task<NoteBundleAppResponseDto> AddNoteAsync(NavigationId navigationId)
  {
    // Generate new note id
    NoteId noteId = await NoteRepository.GenerateUniqueNoteIdAsync();

    // Database
    Note note = NoteFactory.CreateDefaultNote(noteId, navigationId);
    CreateNoteDbRequestDto noteDbDto = NoteMappers.ToDbDto(note);
    CreateNoteViewStateDbRequestDto noteViewStateDto = NoteFactory.CreateDefaultNoteViewStateDto(noteId);

    var noteDbResponseDto = await NoteRepository.AddNoteAsync(noteDbDto);
    var noteViewStateDbResponseDto =  await NoteRepository.AddNoteViewStateAsync(noteViewStateDto);

    // Search Index
    NoteSearchDocumentDto noteSearchDocumentDto = NoteFactory.CreateDefaultNoteSearchDocumentDto(noteId);
    await NoteSearcher.WriteNoteIndexAsync(noteSearchDocumentDto);

    return NoteMappers.ToAppDto(noteDbResponseDto, noteViewStateDbResponseDto);
  }
}
