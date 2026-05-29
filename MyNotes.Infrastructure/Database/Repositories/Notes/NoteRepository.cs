using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MyNotes.Application.Contracts.Database.Dtos.Notes;
using MyNotes.Application.Contracts.Database.Queries.Notes;
using MyNotes.Application.Contracts.Database.Repositories.Notes;
using MyNotes.Domain.Entities.Notes;
using MyNotes.Domain.ValueObjects;
using MyNotes.Infrastructure.Database.Core;
using MyNotes.Infrastructure.Database.Entities.Notes;
using MyNotes.Infrastructure.Mappers;

namespace MyNotes.Infrastructure.Database.Repositories.Notes;

internal class NoteRepository : INoteRepository
{
  private readonly IDbContextFactory<AppDbContext> DbContextFactory;

  public NoteRepository(IDbContextFactory<AppDbContext> dbContextFactory)
  {
    DbContextFactory = dbContextFactory;
  }

  public async Task<NoteId> GenerateUniqueNoteIdAsync()
  {
    await using var context = await DbContextFactory.CreateDbContextAsync();

    NoteId noteId;
    do
    {
      noteId = NoteId.NewId();
    } while (await context.NoteEntities.AnyAsync(e => e.Id == noteId.Value));

    return noteId;
  }

  public async Task<NoteDbResponseDto?> GetNoteAsync(NoteId noteId)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync();
    return await context.NoteEntities.FirstOrDefaultAsync(e => e.Id == noteId.Value) is NoteEntity entity
      ? NoteMappers.ToDto(entity)
      : null;
  }

  public async Task<NoteViewStateDbResponseDto?> GetNoteViewStateDtoAsync(NoteId noteId)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync();
    return await context.NoteViewStateEntities.FirstOrDefaultAsync(e => e.Id == noteId.Value) is NoteViewStateEntity entity
      ? NoteMappers.ToDto(entity)
      : null;
  }

  public Task<IReadOnlyList<NoteDbAggregateResponseDto>> FindNotesAsync(FindNotesDbQuery findNotesDbQuery)
  {
    throw new System.NotImplementedException();
  }

  public async Task AddNoteAsync(CreateNoteDbRequestDto noteDbDto)
  {
    NoteEntity entity = NoteMappers.ToEntity(noteDbDto);

    await using var context = await DbContextFactory.CreateDbContextAsync();
    context.NoteEntities.Add(entity);
    await context.SaveChangesAsync();
  }

  public async Task AddNoteViewStateAsync(CreateNoteViewStateDbRequestDto noteViewStateDbDto)
  {
    NoteViewStateEntity entity = NoteMappers.ToEntity(noteViewStateDbDto);

    await using var context = await DbContextFactory.CreateDbContextAsync();
    context.NoteViewStateEntities.Add(entity);
    await context.SaveChangesAsync();
  }

  public Task<bool> UpdateNoteAsync(UpdateNoteDbRequestDto updateNoteDbDto, bool updateIfChanged = true)
  {
    throw new System.NotImplementedException();
  }

  public Task<bool> UpdateNoteViewStateAsync(UpdateNoteViewStateDbRequestDto updateNoteViewStateDbDto, bool updateIfChanged = true)
  {
    throw new System.NotImplementedException();
  }

  public Task<bool> DeleteNoteAsync(NoteId noteId)
  {
    throw new System.NotImplementedException();
  }
}
