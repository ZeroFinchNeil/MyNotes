using MyNotes.Application.Contracts.Database.Core;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Notes.Persistence;
using MyNotes.Application.Notes.Commands;
using MyNotes.Domain.Notes;

namespace MyNotes.Application.Notes.Services;

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

  public async Task<NoteDto?> AddNoteAsync(CreateNoteAppCommand createNoteAppCommand, CancellationToken cancellationToken = default)
  {
    // Generate new note id
    NoteId noteId = await NoteRepository.GenerateUniqueNoteIdAsync(cancellationToken);

    // Database
    await using var appDbTransaction = await AppDbTransactionFactory.CreateAsync(cancellationToken);

    try
    {
      Note note = NoteFactory.CreateDefaultNote(noteId, createNoteAppCommand.NavigationId);
      NoteViewStateDto noteViewStateDto = NoteFactory.CreateDefaultNoteViewStateDto(noteId, createNoteAppCommand.Size, createNoteAppCommand.Position);
      NoteDto noteDto = NoteMappers.ToDto(note, noteViewStateDto);

      await NoteRepository.AddNoteAsync(noteDto, appDbTransaction, cancellationToken);

      // Add to Search Index
      NoteSearchDocumentDto noteSearchDocumentDto = NoteFactory.CreateDefaultNoteSearchDocumentDto(noteId);
      var searchIndexResult = await NoteSearcher.WriteNoteIndexAsync(noteSearchDocumentDto, cancellationToken);

      if (searchIndexResult)
      {
        await appDbTransaction.CompleteAsync(true, cancellationToken);
        return noteDto;
      }
      else
      {
        await appDbTransaction.RollbackAsync(CancellationToken.None);
        return null;
      }
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
