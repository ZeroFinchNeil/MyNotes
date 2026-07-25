using MyNotes.Application.Commands.Notes;
using MyNotes.Application.Contracts.Database.Core;
using MyNotes.Application.Contracts.Models.Notes;
using MyNotes.Application.Contracts.Persistence.Notes;
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

  public async Task<NoteBundleDto?> AddNoteAsync(CreateNoteAppCommand createNoteAppCommand, CancellationToken cancellationToken = default)
  {
    // Generate new note id
    NoteId noteId = await NoteRepository.GenerateUniqueNoteIdAsync(cancellationToken);

    // Database
    await using var appDbTransaction = await AppDbTransactionFactory.CreateAsync(cancellationToken);

    try
    {
      Note note = NoteFactory.CreateDefaultNote(noteId, createNoteAppCommand.NavigationId);
      NoteDto noteDto = NoteMappers.ToDto(note);
      NoteViewStateDto noteViewStateDto = NoteFactory.CreateDefaultNoteViewStateDto(noteId, createNoteAppCommand.Size, createNoteAppCommand.Position);
      NoteBundleDto noteBundleDto = new(noteDto, noteViewStateDto);

      await NoteRepository.AddNoteAsync(noteBundleDto, appDbTransaction, cancellationToken);

      // Add to Search Index
      NoteSearchDocumentDto noteSearchDocumentDto = NoteFactory.CreateDefaultNoteSearchDocumentDto(noteId);
      var searchIndexResult = await NoteSearcher.WriteNoteIndexAsync(noteSearchDocumentDto, cancellationToken);

      if (searchIndexResult)
      {
        await appDbTransaction.CompleteAsync(true, cancellationToken);
        return noteBundleDto;
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
