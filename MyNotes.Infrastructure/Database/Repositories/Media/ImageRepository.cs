using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MyNotes.Application.Contracts.Media.Models;
using MyNotes.Application.Contracts.Media.Persistence;
using MyNotes.Domain.Media;
using MyNotes.Domain.Notes;
using MyNotes.Infrastructure.Database.Core;
using MyNotes.Infrastructure.Mappers;

namespace MyNotes.Infrastructure.Database.Repositories.Media;

internal sealed class ImageRepository : IImageRepository
{
  private readonly IDbContextFactory<AppDbContext> DbContextFactory;

  public ImageRepository(IDbContextFactory<AppDbContext> dbContextFactory)
  {
    DbContextFactory = dbContextFactory;
  }

  public async Task<ImageId> GenerateUniqueImageIdAsync(CancellationToken cancellationToken = default)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);

    ImageId imageId;
    do
    {
      imageId = ImageId.NewId();
    } while (await context.ImageEntities.AsNoTracking().AnyAsync(e => e.Id == imageId.Value, cancellationToken));

    return imageId;
  }

  public async Task AttachImageAsync(ImageDto imageDto, CancellationToken cancellationToken = default)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);

    var lastImage = await context.ImageEntities
      .AsNoTracking()
      .Where(e => e.NoteId == imageDto.NoteId.Value)
      .OrderByDescending(e => e.Position)
      .FirstOrDefaultAsync(cancellationToken);
    int position = lastImage is null ? 0 : lastImage.Position + 1;

    context.ImageEntities.Add(ImageMappers.ToEntity(imageDto, position));
    await context.SaveChangesAsync(cancellationToken);
    await NormalizePositionAsync(imageDto.NoteId.Value, cancellationToken);
  }
  public async Task<ImageDto?> GetImageAsync(ImageId imageId, CancellationToken cancellationToken = default)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);
    return (await context.ImageEntities
      .AsNoTracking()
      .SingleOrDefaultAsync(e => e.Id == imageId.Value, cancellationToken))
      ?.ToDto();
  }

  public async Task<IReadOnlyList<ImageDto>> GetImagesByNoteIdAsync(NoteId noteId, CancellationToken cancellationToken = default)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);
    return await context.ImageEntities
      .AsNoTracking()
      .Where(e => e.NoteId == noteId.Value)
      .OrderBy(e => e.Position)
      .Select(e => ImageMappers.ToDto(e))
      .ToListAsync(cancellationToken);
  }

  public async Task DeleteImageAsync(ImageId imageId, CancellationToken cancellationToken = default)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);

    Guid noteId = await context.ImageEntities
      .AsNoTracking()
      .Where(e => e.Id == imageId.Value)
      .Select(e => e.NoteId)
      .SingleAsync(cancellationToken);

    await context.EnqueueOperationAsync(
      operation: () => context.ImageEntities
        .Where(e => e.Id == imageId.Value)
        .ExecuteDelete(),
      defaultValue: 0,
      fallbackValue: 0,
      cancellationToken);

    await NormalizePositionAsync(noteId, cancellationToken);
  }

  public async Task MoveImageAsync(ImageId sourceImageId, ImageId targetImageId, CancellationToken cancellationToken = default)
  {
    if (sourceImageId == targetImageId)
    {
      return;
    }

    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);
    await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

    Guid noteId = await context.ImageEntities
      .AsNoTracking()
      .Where(e => e.Id == sourceImageId.Value)
      .Select(e => e.NoteId)
      .SingleAsync(cancellationToken);

    await NormalizePositionAsync(noteId, cancellationToken);

    var images = await context.ImageEntities
      .Where(e1 =>
        context.ImageEntities.Any(e2 =>
          e2.Id == sourceImageId.Value
          && e2.NoteId == e1.NoteId))
      .OrderBy(e => e.Position)
      .ToListAsync(cancellationToken);

    int sourceIndex = images.FindIndex(e => e.Id == sourceImageId.Value);
    int targetIndex = images.FindIndex(e => e.Id == targetImageId.Value);

    if (sourceIndex < 0 || targetIndex < 0)
    {
      return;
    }

    if (sourceIndex == targetIndex)
    {
      return;
    }

    try
    {
      images[sourceIndex].Position = -1;
      await context.SaveChangesAsync(cancellationToken);

      if (sourceIndex < targetIndex)
      {
        for (int index = sourceIndex + 1; index <= targetIndex; index++)
        {
          images[index].Position = index - 1;
          await context.SaveChangesAsync(cancellationToken);
        }
      }
      else if (sourceIndex > targetIndex)
      {
        for (int index = targetIndex; index < sourceIndex; index++)
        {
          images[index].Position = index + 1;
          await context.SaveChangesAsync(cancellationToken);
        }
      }

      images[sourceIndex].Position = targetIndex;
      await context.SaveChangesAsync(cancellationToken);

      await transaction.CommitAsync(cancellationToken);
    }
    catch
    {
      await transaction.RollbackAsync(cancellationToken);
    }
  }

  private async Task NormalizePositionAsync(Guid noteId, CancellationToken cancellationToken = default)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);
    var images = await context.ImageEntities
      .Where(e => e.NoteId == noteId)
      .OrderBy(e => e.Position)
      .ToListAsync(cancellationToken);

    for (int index = 0; index < images.Count; index++)
    {
      var image = images[index];
      var position = image.Position;
      if (position == index)
      {
        continue;
      }
      else if (position > index)
      {
        image.Position = index;
        await context.SaveChangesAsync(cancellationToken);
      }
      else
      {
        throw new InvalidOperationException();
      }
    }
  }
}