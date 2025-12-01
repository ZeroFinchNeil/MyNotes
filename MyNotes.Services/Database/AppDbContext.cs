using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using MyNotes.Services.Database.Entities;

using Windows.Storage;

namespace MyNotes.Services.Database;

internal sealed class AppDbContext(AppDbContextTaskDispatcher channelService) : DbContext
{
  public DbSet<NavigationEntity> NavigationEntities => Set<NavigationEntity>();

  private readonly AppDbContextTaskDispatcher _channelService = channelService;

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    var localFolder = ApplicationData.Current.LocalFolder.Path;
    string connectionString = new SqliteConnectionStringBuilder()
    {
      DataSource = Path.Combine(localFolder, "data.db"),
      ForeignKeys = true,
      DefaultTimeout = 60
    }.ToString();
    optionsBuilder.UseSqlite(connectionString);
  }

  public override int SaveChanges() => _channelService.EnqueueAsync(Task.Run(base.SaveChanges)).GetAwaiter().GetResult();

  public override int SaveChanges(bool acceptAllChangesOnSuccess)=> _channelService.EnqueueAsync(Task.Run(() => base.SaveChanges(acceptAllChangesOnSuccess))).GetAwaiter().GetResult();

  public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) => _channelService.EnqueueAsync(base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken), cancellationToken);

  public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _channelService.EnqueueAsync(base.SaveChangesAsync(cancellationToken), cancellationToken);
}