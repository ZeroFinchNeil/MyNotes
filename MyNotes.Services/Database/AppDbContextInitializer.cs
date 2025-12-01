using Microsoft.EntityFrameworkCore;

namespace MyNotes.Services.Database;

internal sealed class AppDbContextInitializer
{
  public AppDbContextInitializer(IDbContextFactory<AppDbContext> factory)
  {
    using (var context = factory.CreateDbContext())
    {
      context.Database.EnsureCreated();
    }
  }
}
