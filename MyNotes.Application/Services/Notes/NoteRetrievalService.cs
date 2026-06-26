using MyNotes.Application.Contracts.Database.Dtos.Notes.Common;
using MyNotes.Application.Contracts.Database.Repositories.Notes;
using MyNotes.Application.Contracts.Search.Repositories.Notes;
using MyNotes.Application.Dtos.Notes.Common;
using MyNotes.Application.Dtos.Notes.Queries;
using MyNotes.Application.Mappers;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Services.Notes;

internal sealed partial class NoteRetrievalService
{
  private readonly INoteRepository NoteRepository;
  private readonly INoteSearcher NoteSearcher;

  public NoteRetrievalService(INoteRepository noteRepository, INoteSearcher noteSearcher)
  {
    NoteRepository = noteRepository;
    NoteSearcher = noteSearcher;
  }

  public async Task<NoteBundleAppResponseDto?> GetNoteByIdAsync(NoteId noteId)
  {
    return await NoteRepository.GetNoteByIdAsync(noteId) is NoteBundleDbResponseDto dbDto
      ? dbDto.ToAppDto()
      : null;
  }

  public async Task<IReadOnlyList<NoteBundleAppResponseDto>> GetNotesByParentAsync(NavigationId parentId, bool includeDeleted = false)
  {
    var dbDtos = await NoteRepository.GetNotesByParentAsync(parentId, includeDeleted);
    return [.. dbDtos.Select(NoteMappers.ToAppDto)];
  }

  public async Task<IReadOnlyList<NoteBundleAppResponseDto>> GetBookmarkedNotesAsync(bool includeDeleted = false)
  {
    FindNotesAppQuery findNotesAppQuery = includeDeleted 
      ? new() { IsBookmarked = true, }
      : new() { IsBookmarked = true, IsDeleted = false };
    return await FindNotesAsync(findNotesAppQuery);
  }

  public async Task<IReadOnlyList<NoteBundleAppResponseDto>> GetTrashedNotesAsync()
  {
    FindNotesAppQuery findNotesAppQuery = new() { IsDeleted = true };
    return await FindNotesAsync(findNotesAppQuery);
  }

  public async Task<IReadOnlyList<NoteBundleAppResponseDto>> FindNotesAsync(FindNotesAppQuery findNotesQuery)
  {
    List<NoteBundleAppResponseDto> noteDtos = new();
    var dbResponseDtos = await NoteRepository.FindNotesAsync(NoteMappers.ToDbQuery(findNotesQuery));
    foreach (var dbResponseDto in dbResponseDtos)
    {
      noteDtos.Add(dbResponseDto.ToAppDto());
    }
    return noteDtos.AsReadOnly();
  }

  public async Task<IReadOnlyList<NoteBundleAppResponseDto>> SearchNotesAsync(SearchNotesAppQuery searchNotesAppQuery)
  {
    List<NoteBundleAppResponseDto> noteDtos = new();
    //NoteSearcher.
    return noteDtos.AsReadOnly();
  }
}