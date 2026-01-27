using CommunityToolkit.WinUI.Helpers;

using Microsoft.EntityFrameworkCore;

using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.Database;
using MyNotes.Services.Database.Entities;
using MyNotes.Services.Search;
using MyNotes.Services.Search.Entities;
using MyNotes.Services.Windows;
using MyNotes.Views.Windows;

namespace MyNotes.Services.Notes;

internal sealed partial class NoteService : IDisposable
{
  private readonly IDbContextFactory<AppDbContext> DbContextFactory;
  private readonly WindowService WindowService;
  private readonly SearchService SearchService;

  public NoteService(IDbContextFactory<AppDbContext> dbContextFactory, WindowService windowService, SearchService searchService)
  {
    // DI
    DbContextFactory = dbContextFactory;
    WindowService = windowService;
    SearchService = searchService;
  }

  public bool IsDisposed => _disposed;

  private bool _disposed;
  public void Dispose()
  {
    if (_disposed)
      return;

    _disposed = true;
  }

  public NoteWindow OpenNoteWindow(Note note, bool activate = true)
  {
    NoteWindow noteWindow =
      WindowService.NoteWindows.TryGetValue(note.Id, out var wr)
      && wr.TryGetTarget(out var existingNoteWindow)
      && !existingNoteWindow.IsClosed
      ? existingNoteWindow
      : new(note);

    if (activate)
      noteWindow.Activate();

    return noteWindow;
  }
}

internal sealed partial class NoteService : IDisposable
{
  private readonly Dictionary<NoteId, WeakReference<Note>> NoteCache = new();

  private Note NoteEntityToNote(NoteEntity e)
  {
    NoteId noteId = NoteId.Create(e.Id);
    if (NoteCache.TryGetValue(noteId, out var wr)
        && wr.TryGetTarget(out var existingNote))
    {
      return existingNote;
    }
    else
    {
      Note newNote = new()
      {
        Id = noteId,
        NavigationId = NavigationId.Create(e.Parent),
        Created = e.Created,
        Title = e.Title,
        Body = e.Body,
        Background = e.Background.ToColor(),
        Backdrop = (BackdropKind)e.Backdrop,
        Size = new SizeInt32(e.Width, e.Height),
        Position = new PointInt32(e.PositionX, e.PositionY),
        IsBookmarked = e.IsBookmarked,
        IsDeleted = e.IsDeleted
      };
      NoteCache[noteId] = new WeakReference<Note>(newNote);
      return newNote;
    }
  }

  /// <summary>
  /// 지정한 NoteId에 해당하는 노트를 데이터베이스에서 비동기적으로 검색합니다.
  /// </summary>
  /// <param name="noteId">검색하려는 노트의 NoteId입니다.</param>
  public async Task<Note?> FindNoteAsync(NoteId noteId)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync();
    return context.NoteEntities.Find(noteId.Value) is NoteEntity e ? NoteEntityToNote(e) : null;
  }

  /// <summary>
  /// 입력한 조건에 맞는 모든 노트들을 데이터베이스에서 비동기적으로 검색합니다.
  /// </summary>
  /// <param name="predicate">NoteEntity가 원하는 조건에 해당하면 true를 반환하는 predicate입니다.</param>
  public async Task<IReadOnlyList<Note>> GetNotesAsync(Func<NoteEntity, bool> predicate)
  {
    List<Note> notes;

    await using (var context = await DbContextFactory.CreateDbContextAsync())
    {
      notes = [.. context.NoteEntities
        .Where(predicate)
        .Select(NoteEntityToNote)];
    }
    return notes;
  }

  // 노트 업데이트
  public async Task UpdateNoteEntityAsync(Note note, Action<NoteEntity> action)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync();
    if (await context.NoteEntities.FindAsync(note.Id.Value) is NoteEntity entity)
    {
      action.Invoke(entity);
      await context.SaveChangesAsync();
    }
  }

  public async Task UpdateNoteSearchEntityAsync(Note note)
  {
    NoteSearchEntity entity = new()
    {
      Id = note.Id.Value,
      Title = note.Title,
      Body = note.BodyPlainText
    };
    await SearchService.WriteNoteIndexAsync(entity);
  }

  public async Task CommitSearchIndexAsync()
  {
    await SearchService.CommitAsync();
  }

  // 새 노트 추가 및 DB에 반영
  public async Task<Note?> AddNoteAsync(NavigationUserLeafNode navigation)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync();

    NoteId noteId;
    do
    {
      noteId = NoteId.NewId();
    } while (await context.NoteEntities.AnyAsync(e => e.Id == noteId.Value));

    Note note = new()
    {
      Id = noteId,
      NavigationId = navigation.Id,
      Created = DateTimeOffset.UtcNow,
    };

    NoteEntity entity = new()
    {
      Id = note.Id.Value,
      Parent = note.NavigationId.Value,
      Created = note.Created,
      Modified = note.Modified,
      Title = note.Title,
      Body = note.Body,
      Background = note.Background.ToString(),
      Backdrop = (int)note.Backdrop,
      Width = note.Size.Width,
      Height = note.Size.Height,
      PositionX = note.Position.X,
      PositionY = note.Position.Y,
      IsBookmarked = note.IsBookmarked,
      IsDeleted = note.IsDeleted
    };

    context.NoteEntities.Add(entity);
    await context.SaveChangesAsync();

    NoteSearchEntity searchEntity = new()
    {
      Id = note.Id.Value,
      Title = note.Title,
      Body = note.Body
    };
    await SearchService.WriteNoteIndexAsync(searchEntity);

    return note;
  }
}