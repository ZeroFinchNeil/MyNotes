using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MyNotes.Application.Contracts.Database.Core;
using MyNotes.Application.Contracts.Enums.Notes;
using MyNotes.Application.Contracts.Models.Notes;
using MyNotes.Application.Contracts.Models.Notes.Queries;
using MyNotes.Common.Enums.Modes;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Persistence.Notes;

internal interface INoteRepository
{
  public Task<NoteId> GenerateUniqueNoteIdAsync(CancellationToken cancellationToken = default);

  public Task<NoteDto?> GetNoteByIdAsync(NoteId noteId, CancellationToken cancellationToken = default);

  public Task<NoteViewStateDto?> GetNoteViewStateByIdAsync(NoteId noteId, CancellationToken cancellationToken = default);

  public Task<IReadOnlyList<NoteDto>> GetNotesByParentAsync(NavigationId navigationId, bool includeDeleted = false, CancellationToken cancellationToken = default);

  public Task<NoteProjectionDto> GetNoteFieldValuesAsync(NoteId noteId, NoteGetFields noteGetFields, CancellationToken cancellationToken = default);

  public Task<IReadOnlyList<NoteDto>> FindNotesAsync(NoteFilterDto noteFilterDto, CancellationToken cancellationToken = default);

  public Task AddNoteAsync(NoteDto noteDto, IAppDbTransactionContext appDbTransactionContext, CancellationToken cancellationToken = default);

  public Task<PersistenceMutationStatus> UpdateNoteAsync(NotePatchDto notePatchDto, DateTimeOffset modified, CancellationToken cancellationToken = default);

  public Task<PersistenceMutationStatus> UpdateNoteViewStateAsync(NoteViewStatePatchDto noteViewStatePatchDto, CancellationToken cancellationToken = default);

  public Task<PersistenceMutationStatus> DeleteNoteAsync(NoteId noteId, DeleteMode deleteMode, CancellationToken cancellationToken = default);
}