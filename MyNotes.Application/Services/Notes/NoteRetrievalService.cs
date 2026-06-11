using MyNotes.Application.Contracts.Database.Dtos.Notes;
using MyNotes.Application.Contracts.Database.Repositories.Notes;
using MyNotes.Application.Contracts.Search.Repositories.Notes;
using MyNotes.Application.Dtos.Notes;
using MyNotes.Application.Mappers;
using MyNotes.Application.Queries.Notes;
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

  public async Task<NoteAppResponseDto?> GetNoteAsync(NoteId noteId)
  {
    return await NoteRepository.GetNoteAsync(noteId) is NoteDbResponseDto noteDbDto
      && await NoteRepository.GetNoteViewStateDtoAsync(noteId) is NoteViewStateDbResponseDto noteViewStateDto
      ? NoteMappers.ToAppDto(noteDbDto, noteViewStateDto)
      : null;
  }

  public async Task<IReadOnlyList<NoteAppResponseDto>> FindNotesAsync(FindNotesAppQuery findNotesQuery)
  {
    List<NoteAppResponseDto> noteDtos = new();
    var noteDbAggregateDtos = await NoteRepository.FindNotesAsync(NoteMappers.ToDbQuery(findNotesQuery));
    foreach (var noteDbAggregateDto in noteDbAggregateDtos)
    {
      noteDtos.Add(NoteMappers.ToAppDto(noteDbAggregateDto.NoteDbResponseDto, noteDbAggregateDto.NoteViewStateDbResponseDto));
    }
    return noteDtos.AsReadOnly();
  }

  public async Task<IReadOnlyList<NoteAppResponseDto>> SearchNotesAsync()
  {
    List<NoteAppResponseDto> noteDtos = new();
    //NoteSearcher.
    return noteDtos.AsReadOnly();
  }
}