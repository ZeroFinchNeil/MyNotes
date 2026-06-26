using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MyNotes.Application.Contracts.Database.Dtos.Notes.Common;
using MyNotes.Application.Contracts.Database.Dtos.Notes.Creation;
using MyNotes.Application.Contracts.Database.Dtos.Notes.Modification;
using MyNotes.Application.Contracts.Database.Dtos.Notes.Queries;
using MyNotes.Application.Contracts.Database.Repositories.Notes;
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

  public async Task<NoteId> GenerateUniqueNoteIdAsync(CancellationToken cancellationToken = default)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);

    NoteId noteId;
    do
    {
      noteId = NoteId.NewId();
    } while (await context.NoteEntities.AnyAsync(e => e.Id == noteId.Value, cancellationToken));

    return noteId;
  }
  public Task<NoteBundleDbResponseDto> AddNoteAsync(CreateNoteBundleDbRequestDto createNoteBundleDbRequestDto, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
    //NoteEntity entity = NoteMappers.ToEntity(noteDbDto);

    //await using var context = await DbContextFactory.CreateDbContextAsync();
    //context.NoteEntities.Add(entity);
    //await context.SaveChangesAsync();
  }

  public async Task<NoteBundleDbResponseDto?> GetNoteByIdAsync(NoteId noteId, CancellationToken cancellationToken = default)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);
    return await context.NoteEntities
      .AsNoTracking()
      .Where(e => e.Id == noteId.Value)
      .Join(
        context.NoteViewStateEntities.AsNoTracking(),
        outer => outer.Id,
        inner => inner.Id,
        (outer, inner) => NoteMappers.ToDto(outer, inner))
      .FirstOrDefaultAsync(cancellationToken);
  }

  public async Task<NoteViewStateDbResponseDto?> GetNoteViewStateByIdAsync(NoteId noteId, CancellationToken cancellationToken = default)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);
    return await context.NoteViewStateEntities
      .AsNoTracking()
      .Where(e => e.Id == noteId.Value)
      .Select(e => NoteMappers.ToDto(e))
      .FirstOrDefaultAsync(cancellationToken);
  }

  public async Task<IReadOnlyList<NoteBundleDbResponseDto>> GetNotesByParentAsync(NavigationId parentId, bool includeDeleted = false, CancellationToken cancellationToken = default)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);
    Expression<Func<NoteEntity, bool>> predicate = includeDeleted
      ? e => e.Parent == parentId.Value
      : e => e.Parent == parentId.Value && e.IsDeleted == false;
    return await context.NoteEntities
      .AsNoTracking()
      .Where(predicate)
      .Join(
        context.NoteViewStateEntities.AsNoTracking(),
        outer => outer.Id,
        inner => inner.Id,
        (outer, inner) => NoteMappers.ToDto(outer, inner))
      .ToListAsync(cancellationToken);
  }

  public Task<IReadOnlyList<NoteBundleDbResponseDto>> FindNotesAsync(FindNotesDbQuery findNotesDbQuery, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }

  public Task<bool> UpdateNoteAsync(UpdateNoteDbRequestDto updateNoteDbDto, bool updateIfChanged = true, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }

  public Task<bool> UpdateNoteViewStateAsync(UpdateNoteViewStateDbRequestDto updateNoteViewStateDbDto, bool updateIfChanged = true, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }

  public Task<bool> DeleteNoteAsync(NoteId noteId, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }
}
