using System.Collections.Generic;
using System.Threading.Tasks;

using MyNotes.Application.Contracts.Database.Dtos.Notes.Common;
using MyNotes.Application.Contracts.Database.Dtos.Notes.Creation;
using MyNotes.Application.Contracts.Database.Dtos.Notes.Modification;
using MyNotes.Application.Contracts.Database.Dtos.Notes.Queries;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Repositories.Notes;

internal interface INoteRepository
{
  public Task<NoteId> GenerateUniqueNoteIdAsync();

  public Task<NoteDbResponseDto?> GetNoteByIdAsync(NoteId noteId);

  public Task<NoteViewStateDbResponseDto?> GetNoteViewStateDtoAsync(NoteId noteId);

  public Task<IReadOnlyList<NoteBundleDbResponseDto>> GetNotesByNavigationAsync(NavigationId navigationId);

  public Task<IReadOnlyList<NoteBundleDbResponseDto>> FindNotesAsync(FindNotesDbQuery findNotesDbQuery);

  public Task<NoteDbResponseDto> AddNoteAsync(CreateNoteDbRequestDto noteDbDto);

  public Task<NoteViewStateDbResponseDto> AddNoteViewStateAsync(CreateNoteViewStateDbRequestDto dto);

  public Task<bool> UpdateNoteAsync(UpdateNoteDbRequestDto updateNoteDbDto, bool updateIfChanged = true);

  public Task<bool> UpdateNoteViewStateAsync(UpdateNoteViewStateDbRequestDto updateNoteViewStateDbDto, bool updateIfChanged = true);

  public Task<bool> DeleteNoteAsync(NoteId noteId);
}