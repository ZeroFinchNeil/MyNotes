using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

using MyNotes.Infrastructure.Database.Constants;
using MyNotes.Infrastructure.Database.Entities.Media;
using MyNotes.Infrastructure.Database.Entities.Navigations;
using MyNotes.Infrastructure.Database.Entities.Notes;

using Windows.Storage;

namespace MyNotes.Infrastructure.Database.Core;

internal sealed partial class AppDbContext(AppDbContextTaskDispatcher taskDispatcher) : DbContext
{
  /// <summary>내비게이션 DB 엔티티</summary>
  public DbSet<NavigationEntity> NavigationEntities => Set<NavigationEntity>();

  public DbSet<LeafNavigationViewStateEntity> LeafNavigationViewStateEntities => Set<LeafNavigationViewStateEntity>();

  public DbSet<CompositeNavigationViewStateEntity> CompositeNavigationViewStateEntities => Set<CompositeNavigationViewStateEntity>();

  /// <summary>노트 DB 엔티티</summary>
  public DbSet<NoteEntity> NoteEntities => Set<NoteEntity>();

  /// <summary>노트 뷰 상태 DB 엔티티</summary>
  public DbSet<NoteViewStateEntity> NoteViewStateEntities => Set<NoteViewStateEntity>();

  /// <summary>이미지 메타데이터 DB 엔티티</summary>
  public DbSet<ImageEntity> ImageEntities => Set<ImageEntity>();

  private static readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;

  /// <summary>EFCore(SQLite) 연결 문자열</summary>
  private static readonly string _connectionString = new SqliteConnectionStringBuilder()
  {
    DataSource = Path.Combine(_localFolder.Path, DbCoreSettings.DbFileName),
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
    modelBuilder.Entity<NavigationEntity>(entity =>
    {
      entity.HasIndex(e => new { e.Parent, e.Position }).IsUnique();

      entity.Property(e => e.Id).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
      entity.Property(e => e.IsComposite).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
    });

    modelBuilder.Entity<CompositeNavigationViewStateEntity>(entity =>
    {
      entity.HasOne<NavigationEntity>()
        .WithOne()
        .HasForeignKey<CompositeNavigationViewStateEntity>(e => e.Id)
        .IsRequired()
        .OnDelete(DeleteBehavior.Cascade);

      entity.Property(e => e.Id).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
    });

    modelBuilder.Entity<LeafNavigationViewStateEntity>(entity =>
    {
      entity.HasOne<NavigationEntity>()
        .WithOne()
        .HasForeignKey<LeafNavigationViewStateEntity>(e => e.Id)
        .IsRequired()
        .OnDelete(DeleteBehavior.Cascade);

      entity.Property(e => e.Id).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
    });

    // Notes
    modelBuilder.Entity<NoteEntity>(entity =>
    {
      entity.HasOne<NavigationEntity>()
        .WithMany()
        .HasForeignKey(e => e.Navigation)
        .IsRequired(false)
        .OnDelete(DeleteBehavior.SetNull);

      entity.Property(e => e.Id).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
      entity.Property(e => e.Created).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
    });

    modelBuilder.Entity<NoteViewStateEntity>(entity =>
    {
      entity.HasOne<NoteEntity>()
        .WithOne()
        .HasForeignKey<NoteViewStateEntity>(e => e.Id)
        .IsRequired()
        .OnDelete(DeleteBehavior.Cascade);

      entity.Property(e => e.Id).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
    });

    modelBuilder.Entity<ImageEntity>(entity =>
    {
      entity.HasOne<NoteEntity>()
        .WithMany()
        .HasForeignKey(e => e.NoteId)
        .IsRequired()
        .OnDelete(DeleteBehavior.Cascade);

      entity.Property(e => e.Id).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
      entity.Property(e => e.NoteId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
    });
  }

  public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) => taskDispatcher.EnqueueOperationAsync(
    operation: () => !cancellationToken.IsCancellationRequested ? base.SaveChanges(acceptAllChangesOnSuccess) : 0,
    fallbackValue: 0,
    cancellationToken: cancellationToken);

  public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => taskDispatcher.EnqueueOperationAsync(
    operation: () => !cancellationToken.IsCancellationRequested ? base.SaveChanges() : 0,
    fallbackValue: 0,
    cancellationToken: cancellationToken);

  public Task<T?> EnqueueOperationAsync<T>(Func<T?> operation, T? defaultValue = default, T? fallbackValue = default, CancellationToken cancellationToken = default) where T : notnull =>
    taskDispatcher.EnqueueOperationAsync(
      operation: !cancellationToken.IsCancellationRequested ? operation : () => defaultValue,
      fallbackValue: fallbackValue,
      cancellationToken: cancellationToken);
}