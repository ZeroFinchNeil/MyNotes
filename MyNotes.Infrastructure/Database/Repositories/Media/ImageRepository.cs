using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MyNotes.Application.Contracts.Media.Models;
using MyNotes.Application.Contracts.Media.Persistence;
using MyNotes.Domain.Media;
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

    var last = await context.ImageEntities
      .AsNoTracking()
      .Where(e => e.NoteId == imageDto.NoteId.Value)
      .OrderByDescending(e => e.Position)
      .FirstOrDefaultAsync(cancellationToken);
    int position = last is null ? 0 : last.Position + 1;

    context.ImageEntities.Add(ImageMappers.ToEntity(imageDto, position));
    await context.SaveChangesAsync(cancellationToken);
  }
}