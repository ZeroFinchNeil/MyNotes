using MyNotes.Application.Commands.Notes;
using MyNotes.Application.Contracts.Enums.Notes;
using MyNotes.Application.Contracts.Models.Notes;
using MyNotes.Application.Contracts.Persistence;
using MyNotes.Application.Contracts.Persistence.Notes;
using MyNotes.Application.Mappers;
using MyNotes.Application.Results;
using MyNotes.Common.Enums.Modes;
using MyNotes.Domain.ValueObjects;
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

  public async Task<AppUpdateStatus> UpdateNoteAsync(UpdateNoteAppCommand appCommand, CancellationToken cancellationToken = default)
  {
    NotePatchDto notePatchDto = appCommand.NotePatchDto;
    var id = notePatchDto.Id;
    if (notePatchDto.IsEmpty)
    {
      return AppUpdateStatus.Unchanged;
    }

    var updateResult = await NoteRepository.UpdateNoteAsync(notePatchDto, appCommand.Modified, cancellationToken);

    if (updateResult is PersistenceMutationStatus.Expired or PersistenceMutationStatus.Unchanged)
    {
      return AppUpdateStatus.Unchanged;
    }

    if (updateResult is PersistenceMutationStatus.NotFound)
    {
      return AppUpdateStatus.TargetNotFound;
    }

    if (notePatchDto.Title.HasValue || notePatchDto.Body.HasValue)
    {
      NoteProjectionDto noteProjectionDto = await NoteRepository.GetNoteFieldValuesAsync(id, NoteGetFields.Title | NoteGetFields.Body, cancellationToken);
      if (noteProjectionDto.Title.TryGet(out var title) && noteProjectionDto.Body.TryGet(out var body))
      {
        await NoteSearcher.WriteNoteIndexAsync(NoteMappers.ToSearchDocumentDto(id, title, RtfTextConverter.ToPlainText(body)), cancellationToken);
      }
    }

    return AppUpdateStatus.Succeeded;
  }

  public async Task<AppUpdateStatus> UpdateNoteViewStateAsync(UpdateNoteViewStateAppCommand appCommand, CancellationToken cancellationToken = default)
  {
    NoteViewStatePatchDto noteViewStatePatchDto = appCommand.NoteViewStatePatchDto;
    if (noteViewStatePatchDto.IsEmpty)
    {
      return AppUpdateStatus.Unchanged;
    }

    return await NoteRepository.UpdateNoteViewStateAsync(noteViewStatePatchDto, cancellationToken) switch
    {
      PersistenceMutationStatus.Applied => AppUpdateStatus.Succeeded,
      PersistenceMutationStatus.Expired or PersistenceMutationStatus.Unchanged => AppUpdateStatus.Unchanged,
      PersistenceMutationStatus.NotFound => AppUpdateStatus.TargetNotFound,
      _ => throw new InvalidOperationException()
    };
  }

  public async Task<AppUpdateStatus> DeleteNoteAsync(DeleteNoteAppCommand deleteNoteAppCommand, CancellationToken cancellationToken = default)
  {
    NoteId noteId = deleteNoteAppCommand.NoteId;
    DeleteMode deleteMode = deleteNoteAppCommand.DeleteMode;
    PersistenceMutationStatus deleteResult = await NoteRepository.DeleteNoteAsync(noteId, deleteMode, cancellationToken);

    if (deleteResult is PersistenceMutationStatus.Expired or PersistenceMutationStatus.Unchanged)
    {
      return AppUpdateStatus.Unchanged;
    }

    if (deleteResult is PersistenceMutationStatus.NotFound)
    {
      return AppUpdateStatus.TargetNotFound;
    }

    if (deleteMode is DeleteMode.Permanent)
    {
      await NoteSearcher.DeleteNoteIndexAsync(noteId, cancellationToken);
    }

    return AppUpdateStatus.Succeeded;
  }

  public Task CommitSearchIndexAsync(CancellationToken cancellationToken = default) => NoteSearcher.CommitAsync(cancellationToken);
}
