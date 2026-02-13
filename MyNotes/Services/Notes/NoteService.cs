using CommunityToolkit.WinUI.Helpers;

using Microsoft.EntityFrameworkCore;

using MyNotes.Common.Interop;
using MyNotes.Constants;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.Database;
using MyNotes.Services.Database.Entities;
using MyNotes.Services.Search;
using MyNotes.Services.Search.Entities;
using MyNotes.Services.Settings;
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

  public bool Disposed { get; private set; }

  public void Dispose()
  {
    if (Disposed)
      return;

    Disposed = true;
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

  public async Task OpenNoteWindowsForOpenEntities()
  {
    foreach (var note in await GetNotesAsync(e => e.IsWindowOpen))
    {
      OpenNoteWindow(note);
    }
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
        IsDeleted = e.IsDeleted,
        IsWindowOpen = e.IsWindowOpen
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

  /// <summary>
  /// <para>Asynchronously updates a note entity in the database by applying a specified action to it. If no entity with the specified note id exists, the action is not invoked and no changes are made.</para>
  /// <para>노트 엔티티를 주어진 액션에 따라 데이터베이스에 비동기 업데이트합니다. 데이터베이스에 일치하는 id를 가진 엔티티가 없으면 액션이 실행되지 않고 변경사항이 저장되지 않습니다.</para>
  /// </summary>
  /// <param name="action">
  /// <para>An action to perform on the found note entity.</para>
  /// <para>일치하는 노트 엔티티에서 수행해야 할 업데이트를 포함한 액션입니다.</para>
  /// </param>
  public Task UpdateNoteEntityAsync(Note note, Action<NoteEntity> action) => UpdateNoteEntityAsync(note.Id, action);

  public async Task UpdateNoteEntityAsync(NoteId noteId, Action<NoteEntity> action)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync();
    if (await context.NoteEntities.FindAsync(noteId.Value) is NoteEntity entity)
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

    if (WindowService.TryGetFocusedWindow(out var focusedWindow, out var hWnd)
      && NativeMethods.GetMonitorInfoForWindow(hWnd) is NativeMethods.MONITORINFOEX monitorInfo)
    {
      var rect = monitorInfo.rcWork;
      int monitorWidth = rect.Right - rect.Left;
      int monitorHeight = rect.Bottom - rect.Top;
      int padding = 10;
      Range h1 = new(rect.Left + padding, rect.Left + (monitorWidth - note.Size.Width) / 2);
      //Range h2 = new(rect.Right - (monitorWidth + note.Size.Width) / 2, rect.Right - padding);
      Range v1 = new(rect.Top + padding, rect.Top + (monitorHeight - note.Size.Height) / 2);
      //Range v2 = new(rect.Bottom - (monitorHeight + note.Size.Height) / 2, rect.Bottom - padding);

      Random random = new();
      int positionX = h1.Start.Value < h1.End.Value ? random.Next(h1.Start.Value, h1.End.Value) : h1.Start.Value;
      int positionY = v1.Start.Value < v1.End.Value ? random.Next(v1.Start.Value, v1.End.Value) : v1.Start.Value;
      note.Position = new PointInt32(positionX, positionY);
    }

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
      IsDeleted = note.IsDeleted,
      IsWindowOpen = false
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