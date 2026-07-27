using Microsoft.EntityFrameworkCore;

using MyNotes.Application.Contracts.Persistence.Media;
using MyNotes.Infrastructure.Database.Core;

namespace MyNotes.Infrastructure.Database.Repositories.Media;

internal sealed class ImageRepository : IImageRepository
{
  private readonly IDbContextFactory<AppDbContext> DbContextFactory;

  public ImageRepository(IDbContextFactory<AppDbContext> dbContextFactory)
  {
    DbContextFactory = dbContextFactory;
  }
}