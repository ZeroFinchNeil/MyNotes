using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MyNotes.Application.Contracts.Database.Core;
using MyNotes.Application.Contracts.Notes.Models.Common;
using MyNotes.Application.Contracts.Notes.Models.Creation;
using MyNotes.Application.Contracts.Notes.Models.Modification;
using MyNotes.Application.Contracts.Notes.Models.Queries;
using MyNotes.Application.Contracts.Notes.Models.Retrieval;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Notes.Persistence;

internal interface INoteRepository
{
  public Task<NoteId> GenerateUniqueNoteIdAsync(CancellationToken cancellationToken = default);

  public Task<NoteBundleDbResponseDto?> GetNoteByIdAsync(NoteId noteId, CancellationToken cancellationToken = default);

  public Task<NoteViewStateDbResponseDto?> GetNoteViewStateByIdAsync(NoteId noteId, CancellationToken cancellationToken = default);

  public Task<IReadOnlyList<NoteBundleDbResponseDto>> GetNotesByParentAsync(NavigationId navigationId, bool includeDeleted = false, CancellationToken cancellationToken = default);

  public Task<GetNoteFieldValuesDbResponseDto> GetNoteFieldValuesAsync(GetNoteFieldValuesDbRequestDto getFieldsDbRequestDto, CancellationToken cancellationToken = default);

  public Task<IReadOnlyList<NoteBundleDbResponseDto>> FindNotesAsync(FindNotesDbQuery findDbQuery, CancellationToken cancellationToken = default);

  public Task<NoteBundleDbResponseDto> AddNoteAsync(CreateNoteBundleDbRequestDto createDbRequestDto, IAppDbTransactionContext appDbTransactionContext, CancellationToken cancellationToken = default);

  public Task<UpdateNoteDbResponseDto> UpdateNoteAsync(UpdateNoteDbRequestDto updateDbRequestDto, bool updateIfChanged = true, CancellationToken cancellationToken = default);

  public Task<UpdateNoteViewStateDbResponseDto> UpdateNoteViewStateAsync(UpdateNoteViewStateDbRequestDto updateDbRequestDto, bool updateIfChanged = true, CancellationToken cancellationToken = default);

  public Task<bool> DeleteNoteAsync(DeleteNoteDbRequestDto deleteDbRequestDto, CancellationToken cancellationToken = default);
}