using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MyNotes.Application.Contracts.Database.Core;
using MyNotes.Application.Contracts.Database.Dtos.Notes.Common;
using MyNotes.Application.Contracts.Database.Dtos.Notes.Creation;
using MyNotes.Application.Contracts.Database.Dtos.Notes.Modification;
using MyNotes.Application.Contracts.Database.Dtos.Notes.Queries;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Repositories.Notes;

internal interface INoteRepository
{
  public Task<NoteId> GenerateUniqueNoteIdAsync(CancellationToken cancellationToken = default);

  public Task<NoteBundleDbResponseDto?> GetNoteByIdAsync(NoteId noteId, CancellationToken cancellationToken = default);

  public Task<NoteViewStateDbResponseDto?> GetNoteViewStateByIdAsync(NoteId noteId, CancellationToken cancellationToken = default);

  public Task<IReadOnlyList<NoteBundleDbResponseDto>> GetNotesByParentAsync(NavigationId parentId, bool includeDeleted = false, CancellationToken cancellationToken = default);

  public Task<IReadOnlyList<NoteBundleDbResponseDto>> FindNotesAsync(FindNotesDbQuery findNotesDbQuery, CancellationToken cancellationToken = default);

  public Task<NoteBundleDbResponseDto> AddNoteAsync(CreateNoteBundleDbRequestDto createNoteBundleDbRequestDto, IAppDbTransactionContext appDbTransactionContext, CancellationToken cancellationToken = default);

  public Task<bool> UpdateNoteAsync(UpdateNoteDbRequestDto updateNoteDbDto, bool updateIfChanged = true, CancellationToken cancellationToken = default);

  public Task<bool> UpdateNoteViewStateAsync(UpdateNoteViewStateDbRequestDto updateNoteViewStateDbDto, bool updateIfChanged = true, CancellationToken cancellationToken = default);

  public Task<bool> DeleteNoteAsync(NoteId noteId, CancellationToken cancellationToken = default);
}