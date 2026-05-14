using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using MyNotes.Infrastructure.Database.Entities.Navigations;
using MyNotes.Infrastructure.Database.Entities.Notes;

using Windows.Storage;

namespace MyNotes.Infrastructure.Database.Core;

internal sealed partial class AppDbContext(AppDbContextTaskDispatcher taskDispatcher) : DbContext
{
  /// <summary>내비게이션 DB 엔티티</summary>
  public DbSet<UserNavigationEntity> NavigationEntities => Set<UserNavigationEntity>();

  public DbSet<UserLeafNavigationViewStateEntity> NavigationViewStateEntities => Set<UserLeafNavigationViewStateEntity>();

  /// <summary>노트 DB 엔티티</summary>
  public DbSet<NoteEntity> NoteEntities => Set<NoteEntity>();

  /// <summary>노트 뷰 상태 DB 엔티티</summary>
  public DbSet<NoteViewStateEntity> NoteViewStateEntities => Set<NoteViewStateEntity>();

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

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Navigations
    modelBuilder.Entity<UserNavigationEntity>()
      .HasIndex(e => new { e.Parent, e.Position })
      .IsUnique();

    modelBuilder.Entity<UserCompositeNavigationViewStateEntity>()
      .HasOne(e => e.Navigation)
      .WithOne()
      .HasForeignKey<UserCompositeNavigationViewStateEntity>(e => e.Id)
      .IsRequired()
      .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<UserLeafNavigationViewStateEntity>()
      .HasOne(e => e.Navigation)
      .WithOne()
      .HasForeignKey<UserLeafNavigationViewStateEntity>(e => e.Id)
      .IsRequired()
      .OnDelete(DeleteBehavior.Cascade);

    // Notes
    modelBuilder.Entity<NoteViewStateEntity>()
      .HasOne(e => e.Note)
      .WithOne()
      .HasForeignKey<NoteViewStateEntity>(e => e.Id)
      .IsRequired()
      .OnDelete(DeleteBehavior.Cascade);
  }

  public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) => taskDispatcher.EnqueueSaveChangesAsync(
    saveChanges: () => !cancellationToken.IsCancellationRequested ? base.SaveChanges(acceptAllChangesOnSuccess) : 0,
    cancellationToken: cancellationToken);

  public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => taskDispatcher.EnqueueSaveChangesAsync(
    saveChanges: () => !cancellationToken.IsCancellationRequested ? base.SaveChanges() : 0,
    cancellationToken: cancellationToken);
}