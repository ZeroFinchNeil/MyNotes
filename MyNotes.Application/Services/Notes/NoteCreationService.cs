using MyNotes.Application.Contracts.Database.Dtos.Notes.Creation;
using MyNotes.Application.Contracts.Database.Repositories.Notes;
using MyNotes.Application.Contracts.Search.Dtos.Notes;
using MyNotes.Application.Contracts.Search.Repositories.Notes;
using MyNotes.Application.Dtos.Notes.Common;
using MyNotes.Application.Mappers;
using MyNotes.Common.Querying;
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

  public async Task<NoteBundleAppResponseDto> AddNoteAsync(NavigationId navigationId, CancellationToken cancellation = default)
  {
    // Generate new note id
    NoteId noteId = await NoteRepository.GenerateUniqueNoteIdAsync(cancellation);

    // Database
    Note note = NoteFactory.CreateDefaultNote(noteId, navigationId);
    CreateNoteBundleDbRequestDto appRequestDto = new(noteDto: note.ToCreateDbDto(), viewStateDto: NoteFactory.CreateDefaultNoteViewStateDto(noteId));

    var dbResponseDto = await NoteRepository.AddNoteAsync(appRequestDto, cancellation);

    // Add to Search Index
    NoteSearchDocumentDto noteSearchDocumentDto = NoteFactory.CreateDefaultNoteSearchDocumentDto(noteId);
    _ = await NoteSearcher.WriteNoteIndexAsync(noteSearchDocumentDto, cancellation);

    return dbResponseDto.ToAppDto();
  }
}
