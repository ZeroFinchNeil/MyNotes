using MyNotes.Application.Contracts.Database.Dtos.Notes.Common;
using MyNotes.Application.Contracts.Database.Enums.Notes;
using MyNotes.Application.Contracts.Database.Repositories.Notes;
using MyNotes.Application.Contracts.Search.Repositories.Notes;
using MyNotes.Application.Dtos.Notes.Modification;
using MyNotes.Application.Mappers;

namespace MyNotes.Application.Services.Notes;

internal sealed partial class NoteModificationService
{
  private readonly INoteRepository NoteRepository;
  private readonly INoteSearcher NoteSearcher;

  public NoteModificationService(INoteRepository noteRepositoryService, INoteSearcher noteSearcher)
  {
    NoteRepository = noteRepositoryService;
    NoteSearcher = noteSearcher;
  }

  public async Task<bool> UpdateNoteAsync(UpdateNoteAppRequestDto updateNoteDto)
  {
    NoteUpdateFields noteUpdateField = updateNoteDto.NoteUpdateField;
    if (noteUpdateField is NoteUpdateFields.None)
    {
      return false;
    }

    bool dbSuccess = await NoteRepository.UpdateNoteAsync(NoteMappers.ToDbDto(updateNoteDto));

    bool searchSuccess = true;

    if (noteUpdateField.HasFlag(NoteUpdateFields.Title) || noteUpdateField.HasFlag(NoteUpdateFields.Body))
    {
      searchSuccess = await NoteRepository.GetNoteByIdAsync(updateNoteDto.Id) is NoteDbResponseDto noteDbDto && await NoteSearcher.WriteNoteIndexAsync(NoteMappers.ToSearchDocumentDto(noteDbDto));
    }

    return dbSuccess && searchSuccess;
  }

  public async Task<bool> UpdateNoteViewStateAsync(UpdateNoteViewStateAppRequestDto updateNoteViewStateDto)
  {
    NoteViewStateUpdateFields updateNoteViewStateField = updateNoteViewStateDto.NoteViewStateUpdateField;
    return updateNoteViewStateField is not NoteViewStateUpdateFields.None && await NoteRepository.UpdateNoteViewStateAsync(NoteMappers.ToDbDto(updateNoteViewStateDto));
  }
}
