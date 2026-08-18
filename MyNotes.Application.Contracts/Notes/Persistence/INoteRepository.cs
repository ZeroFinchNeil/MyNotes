using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MyNotes.Application.Contracts.Database.Core;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Persistence;
using MyNotes.Common.Enums.Modes;
using MyNotes.Domain.Navigations;
using MyNotes.Domain.Notes;

namespace MyNotes.Application.Contracts.Notes.Persistence;

internal interface INoteRepository
{
  public Task<NoteId> GenerateUniqueNoteIdAsync(CancellationToken cancellationToken = default);

  public Task<NoteDto?> GetNoteByIdAsync(NoteId noteId, CancellationToken cancellationToken = default);

  public Task<IReadOnlyCollection<NoteDto>> GetNotesByIdsAsync(IEnumerable<NoteId> noteIds, CancellationToken cancellationToken = default);

  public Task<NoteViewStateDto?> GetNoteViewStateByIdAsync(NoteId noteId, CancellationToken cancellationToken = default);

  public Task<IReadOnlyList<NoteDto>> GetNotesByParentAsync(NavigationId navigationId, bool includeDeleted = false, CancellationToken cancellationToken = default);

  public Task<NoteProjectionDto> GetNoteFieldValuesAsync(NoteId noteId, NoteProjectionFields noteGetFields, CancellationToken cancellationToken = default);

  public Task<IReadOnlyList<NoteDto>> FindNotesAsync(NoteFilterDto noteFilterDto, CancellationToken cancellationToken = default);

  public Task AddNoteAsync(NoteDto noteDto, IAppDbTransactionContext appDbTransactionContext, CancellationToken cancellationToken = default);

  public Task<PersistenceMutationStatus> UpdateNoteAsync(NotePatchDto notePatchDto, DateTimeOffset modified, CancellationToken cancellationToken = default);

  public Task<PersistenceMutationStatus> UpdateNoteViewStateAsync(NoteViewStatePatchDto noteViewStatePatchDto, CancellationToken cancellationToken = default);

  public Task<PersistenceMutationStatus> DeleteNoteAsync(NoteId noteId, DeleteMode deleteMode, CancellationToken cancellationToken = default);
}