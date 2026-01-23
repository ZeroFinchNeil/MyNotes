using System;
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
  /// <summary>내비게이션 DB 엔티티</summary>
  public DbSet<NavigationEntity> NavigationEntities => Set<NavigationEntity>();

  /// <summary>노트 DB 엔티티</summary>
  public DbSet<NoteEntity> NoteEntities => Set<NoteEntity>();

  private readonly AppDbContextTaskDispatcher _channelService = channelService;

  private static readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;

  /// <summary>EFCore(SQLite) 연결 문자열</summary>
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

  public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) => _channelService.EnqueueSaveChangesAsync(
    saveChanges: () => !cancellationToken.IsCancellationRequested ? base.SaveChanges(acceptAllChangesOnSuccess) : 0,
    cancellationToken: cancellationToken);

  public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _channelService.EnqueueSaveChangesAsync(
    saveChanges: () => !cancellationToken.IsCancellationRequested ? base.SaveChanges() : 0,
    cancellationToken: cancellationToken);
}