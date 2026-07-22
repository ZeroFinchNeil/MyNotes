using MyNotes.Application.Contracts.Database.Dtos.Notes.Retrieval;
using MyNotes.Application.Contracts.Database.Enums.Notes;
using MyNotes.Application.Contracts.Database.Repositories.Notes;
using MyNotes.Application.Contracts.Search.Repositories.Notes;
using MyNotes.Application.Dtos.Notes.Modification;
using MyNotes.Application.Mappers;
using MyNotes.Common.Enums.Modes;
using MyNotes.Shell.Contracts.Converters;

namespace MyNotes.Application.Services.Notes;

internal sealed partial class NoteModificationService
{
  private readonly INoteRepository NoteRepository;
  private readonly INoteSearcher NoteSearcher;
  private readonly IRtfTextConverter RtfTextConverter;

  public NoteModificationService(INoteRepository noteRepositoryService, INoteSearcher noteSearcher, IRtfTextConverter rtfTextConverter)
  {
    NoteRepository = noteRepositoryService;
    NoteSearcher = noteSearcher;
    RtfTextConverter = rtfTextConverter;
  }

  public async Task<UpdateNoteAppResponseDto> UpdateNoteAsync(UpdateNoteAppRequestDto updateAppRequestDto, CancellationToken cancellationToken = default)
  {
    var id = updateAppRequestDto.Id;
    if (updateAppRequestDto.IsEmpty)
    {
      return new() { Id = id };
    }

    var updateDbResponseDto = await NoteRepository.UpdateNoteAsync(NoteMappers.ToDbDto(updateAppRequestDto, DateTimeOffset.UtcNow), true, cancellationToken);

    if (updateDbResponseDto.Title.HasValue || updateAppRequestDto.Body.HasValue)
    {
      GetNoteFieldValuesDbRequestDto getDbRequestDto = new()
      {
        GetFields = NoteGetFields.Title | NoteGetFields.Body,
        Id = id
      };
      var getDbResponseDto = await NoteRepository.GetNoteFieldValuesAsync(getDbRequestDto, cancellationToken);
      if (getDbResponseDto.Title.TryGet(out var title) && getDbResponseDto.Body.TryGet(out var body))
      {
        await NoteSearcher.WriteNoteIndexAsync(NoteMappers.ToSearchDocumentDto(id, title, RtfTextConverter.ToPlainText(body)), cancellationToken);
      }
    }

    return NoteMappers.ToAppDto(updateDbResponseDto);
  }

  public async Task<UpdateNoteViewStateAppResponseDto> UpdateNoteViewStateAsync(UpdateNoteViewStateAppRequestDto updateAppRequestDto, CancellationToken cancellationToken = default)
  {
    var id = updateAppRequestDto.Id;
    if (updateAppRequestDto.IsEmpty)
    {
      return new() { Id = id };
    }
    var updateDbResponseDto = await NoteRepository.UpdateNoteViewStateAsync(NoteMappers.ToDbDto(updateAppRequestDto), true, cancellationToken);
    return NoteMappers.ToAppDto(updateDbResponseDto);
  }

  public async Task<bool> DeleteNoteAsync(DeleteNoteAppRequestDto deleteAppRequestDto, CancellationToken cancellationToken = default)
  {
    if (await NoteRepository.DeleteNoteAsync(NoteMappers.ToDbDto(deleteAppRequestDto), cancellationToken))
    {
      if (deleteAppRequestDto.DeleteMode is DeleteMode.Permanent)
      {
        await NoteSearcher.DeleteNoteIndexAsync(deleteAppRequestDto.Id, cancellationToken);
      }
      return true;
    }

    return false;
  }

  public Task CommitSearchIndexAsync(CancellationToken cancellationToken = default) => NoteSearcher.CommitAsync(cancellationToken);
}
