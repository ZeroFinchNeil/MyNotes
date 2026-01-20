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
  public DbSet<NoteEntity> NoteEntities => Set<NoteEntity>();

  private readonly AppDbContextTaskDispatcher _channelService = channelService;

  private static readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;
  private static readonly string _connectionString = new SqliteConnectionStringBuilder()
  {
    DataSource = Path.Combine(_localFolder.Path, "data.db"),
    ForeignKeys = true,
    DefaultTimeout = 60
  }.ToString();

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    optionsBuilder.UseSqlite(_connectionString);
  }

  public override int SaveChanges() => _channelService.EnqueueSaveChangesAsync(() => Task.Run(base.SaveChanges)).GetAwaiter().GetResult();

  public override int SaveChanges(bool acceptAllChangesOnSuccess) => _channelService.EnqueueSaveChangesAsync(() => Task.Run(() => base.SaveChanges(acceptAllChangesOnSuccess))).GetAwaiter().GetResult();

  public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) => _channelService.EnqueueSaveChangesAsync(() => base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken), cancellationToken);

  public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _channelService.EnqueueSaveChangesAsync(() => base.SaveChangesAsync(cancellationToken), cancellationToken);
}