using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MyNotes.Application.Contracts.Media.Models;
using MyNotes.Application.Contracts.Media.Persistence;
using MyNotes.Infrastructure.Database.Core;

namespace MyNotes.Infrastructure.Database.Repositories.Media;

internal sealed class ImageRepository : IImageRepository
{
  private readonly IDbContextFactory<AppDbContext> DbContextFactory;

  public ImageRepository(IDbContextFactory<AppDbContext> dbContextFactory)
  {
    DbContextFactory = dbContextFactory;
  }

  public Task AttachImageAsync(ImageDto imageDto)
  {
    throw new System.NotImplementedException();
  }
}