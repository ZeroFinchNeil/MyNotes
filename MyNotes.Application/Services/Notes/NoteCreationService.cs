using MyNotes.Application.Contracts.Database.Core;
using MyNotes.Application.Contracts.Notes.Models.Creation;
using MyNotes.Application.Contracts.Notes.Models.Search;
using MyNotes.Application.Contracts.Notes.Persistence;
using MyNotes.Application.Dtos.Notes.Common;
using MyNotes.Application.Dtos.Notes.Creation;
using MyNotes.Application.Mappers;
using MyNotes.Domain.Entities.Notes;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Services.Notes;

internal sealed partial class NoteCreationService
{
  private readonly INoteRepository NoteRepository;
  private readonly IAppDbTransactionFactory AppDbTransactionFactory;
  private readonly NoteFactory NoteFactory;
  private readonly INoteSearcher NoteSearcher;

  public NoteCreationService(INoteRepository noteRepository, IAppDbTransactionFactory appDbTransactionFactory, NoteFactory noteFactory, INoteSearcher noteSearcher)
  {
    NoteRepository = noteRepository;
    AppDbTransactionFactory = appDbTransactionFactory;
    NoteFactory = noteFactory;
    NoteSearcher = noteSearcher;
  }

  public async Task<NoteBundleAppResponseDto> AddNoteAsync(CreateNoteAppRequestDto appRequestDto, CancellationToken cancellationToken = default)
  {
    // Generate new note id
    NoteId noteId = await NoteRepository.GenerateUniqueNoteIdAsync(cancellationToken);

    // Database
    await using var appDbTransaction = await AppDbTransactionFactory.CreateAsync(cancellationToken);

    try
    {
      Note note = NoteFactory.CreateDefaultNote(noteId, appRequestDto.NavigationId);
      CreateNoteBundleDbRequestDto dbRequestDto = new(noteDto: note.ToCreateDbDto(), viewStateDto: NoteFactory.CreateDefaultNoteViewStateDto(noteId, appRequestDto.Size, appRequestDto.Position));

      var dbResponseDto = await NoteRepository.AddNoteAsync(dbRequestDto, appDbTransaction, cancellationToken);

      // Add to Search Index
      WriteNoteSearchDocumentRequestDto noteSearchDocumentDto = NoteFactory.CreateDefaultNoteSearchDocumentDto(noteId);
      _ = await NoteSearcher.WriteNoteIndexAsync(noteSearchDocumentDto, cancellationToken);

      await appDbTransaction.CompleteAsync(true, cancellationToken);
      return dbResponseDto.ToAppDto();
    }
    catch
    {
      if (!appDbTransaction.IsCompleted && !appDbTransaction.IsRolledBack)
      {
        await appDbTransaction.RollbackAsync(CancellationToken.None);
      }

      throw;
    }
  }
}
