using CommunityToolkit.WinUI.Helpers;

using Microsoft.EntityFrameworkCore;

using MyNotes.AppConstants;
using MyNotes.Common.Interop;
using MyNotes.Helpers;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.App;
using MyNotes.Services.Database;
using MyNotes.Services.Database.Entities;
using MyNotes.Services.Search;
using MyNotes.Services.Search.Entities;
using MyNotes.Services.Settings;
using MyNotes.Views.Windows;

namespace MyNotes.Services.Notes;

internal sealed partial class NoteService : IDisposable
{
  private readonly IDbContextFactory<AppDbContext> DbContextFactory;
  private readonly SettingsService SettingsService;
  private readonly WindowService WindowService;
  private readonly SearchService SearchService;

  public NoteService(IDbContextFactory<AppDbContext> dbContextFactory, SettingsService settingsService, WindowService windowService, SearchService searchService)
  {
    // DI
    DbContextFactory = dbContextFactory;
    SettingsService = settingsService;
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

  /// <summary>
  /// <para>지정된 노트에 대한 창을 열고, 선택적으로 로드 후 활성화합니다. 지정된 노트에 대한 창이 이미 열려 있고 닫히지 않은 경우, 이 메서드는 기존 창을 반환합니다. 그렇지 않으면 새 창을 생성합니다. 창은 <paramref name="activate"/>가 <see langword="true"/>인 경우에만 활성화됩니다.</para>
  /// <para>Opens a window for the specified note, optionally activating it after loading. If a window for the specified note is already open and not closed, this method returns the existing window. Otherwise, a new window is created. The window is activated only if <paramref name="activate"/> is <see langword="true"/>.</para>
  /// </summary>
  /// <param name="note">The note for which to open a window. Cannot be null.</param>
  /// <param name="activate">A value indicating whether the window should be activated after it is loaded. If <see langword="true"/>, the
  /// window is brought to the foreground.</param>
  /// <returns>A <see cref="NoteWindow"/> instance representing the window for the specified note. If a window for the note
  /// already exists and is not closed, the existing window is returned.</returns>
  public async Task<NoteWindow> OpenNoteWindow(Note note, bool activate = true)
  {
    NoteWindow noteWindow =
      WindowService.NoteWindows.TryGetValue(note.Id, out var wr)
      && wr.TryGetTarget(out var existingNoteWindow)
      && !existingNoteWindow.IsClosed
      ? existingNoteWindow
      : new(note);

    if (activate)
    {
      await noteWindow.LoadedTask.Task;
      noteWindow.Activate();
    }

    return noteWindow;
  }

  public async Task<int> OpenNoteWindowsForOpenEntities()
  {
    int result = 0;
    foreach (var note in await GetNotesAsync(e => e.IsWindowOpen))
    {
      await OpenNoteWindow(note);
      result++;
    }
    return result;
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
      Note note = new()
      {
        Id = noteId,
        NavigationId = NavigationId.GetOrCreate(e.Parent),
        Created = e.Created,
        Title = e.Title,
        Body = e.Body,
        BackgroundColor = e.BackgroundColor.ToColor(),
        IsBackgroundImageVisible = e.IsBackgroundImageVisible,
        BackgroundImagePath = e.BackgroundImagePath,
        BackgroundImageOpacity = e.BackgroundImageOpacity,
        BackgroundImageBlur = e.BackgroundImageBlur,
        BackdropKind = (BackdropKind)e.BackdropKind,
        BackdropTintOpacity = e.BackdropTintOpacity,
        BackdropLuminosityOpacity = e.BackdropLuminosityOpacity,
        Size = new SizeInt32(e.Width, e.Height),
        Position = new PointInt32(e.PositionX, e.PositionY),
        IsBookmarked = e.IsBookmarked,
        IsDeleted = e.IsDeleted,
        IsWindowOpen = e.IsWindowOpen,
        IsAlwaysOnTop = e.IsAlwaysOnTop
      };
      NoteCache[noteId] = new WeakReference<Note>(note);
      return note;
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
  /// <para>노트 엔티티를 주어진 액션에 따라 데이터베이스에 비동기 업데이트합니다. 데이터베이스에 일치하는 id를 가진 엔티티가 없으면 액션이 실행되지 않고 변경사항이 저장되지 않습니다.</para>
  /// <para>Asynchronously updates a note entity in the database by applying a specified action to it. If no entity with the specified note id exists, the action is not invoked and no changes are made.</para>
  /// </summary>
  /// <param name="action">
  /// <para>일치하는 노트 엔티티에서 수행해야 할 업데이트를 포함한 액션입니다.</para>
  /// <para>An action to perform on the found note entity.</para>
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

  /// <summary>
  /// <para>새 노트를 생성하고 데이터베이스에 비동기적으로 추가합니다. 노트는 기본 설정으로 초기화되며, 가능한 경우 현재 포커스가 있는 창을 기준으로 위치가 지정됩니다. 노트는 생성 후 검색을 위해 색인화됩니다.</para>
  /// <para>Creates a new note and adds it to the database asynchronously. The note is initialized with default settings and positioned based on the currently focused window, if available. The note is indexed for search after creation.</para> 
  /// </summary>
  /// <param name="navigation">The navigation node to associate with the new note. If null, the note will be created without a navigation link.</param>
  /// <returns>A task that represents the asynchronous operation. The task result contains the newly created note, or null if the
  /// note could not be created.</returns>
  public async Task<Note> AddNoteAsync(NavigationUserLeafNode? navigation)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync();

    NoteId noteId;
    do
    {
      noteId = NoteId.NewId();
    } while (await context.NoteEntities.AnyAsync(e => e.Id == noteId.Value));

    NavigationId navigationId = navigation?.Id ?? NavigationId.Empty;

    Note note = new()
    {
      Id = noteId,
      NavigationId = navigationId,
      Created = DateTimeOffset.UtcNow,
      BackgroundColor = SettingsService.Load(AppSettingsDescriptors.NoteBackground).ToColor(),
      IsBackgroundImageVisible = false,
      BackgroundImagePath = null,
      BackgroundImageOpacity = 1.0,
      BackgroundImageBlur = 0.0,
      BackdropKind = (BackdropKind)SettingsService.Load(AppSettingsDescriptors.NoteBackdropKind),
      BackdropTintOpacity = 0.5,
      BackdropLuminosityOpacity = 0.0,
      Size = SettingsService.Load(AppSettingsDescriptors.NoteSize).SizeInt32,
      IsBookmarked = false,
      IsDeleted = false,
      IsWindowOpen = false,
      IsAlwaysOnTop = false,
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
      BackgroundColor = note.BackgroundColor.ToString(),
      IsBackgroundImageVisible = note.IsBackgroundImageVisible,
      BackgroundImagePath = note.BackgroundImagePath,
      BackgroundImageOpacity = note.BackgroundImageOpacity,
      BackgroundImageBlur = note.BackgroundImageBlur,
      BackdropKind = (int)note.BackdropKind,
      BackdropTintOpacity = note.BackdropTintOpacity,
      BackdropLuminosityOpacity = note.BackdropLuminosityOpacity,
      Width = note.Size.Width,
      Height = note.Size.Height,
      PositionX = note.Position.X,
      PositionY = note.Position.Y,
      IsBookmarked = note.IsBookmarked,
      IsDeleted = note.IsDeleted,
      IsWindowOpen = note.IsWindowOpen,
      IsAlwaysOnTop = note.IsAlwaysOnTop
    };

    context.NoteEntities.Add(entity);
    await context.SaveChangesAsync();

    NoteSearchEntity searchEntity = new()
    {
      Id = note.Id.Value,
      Title = note.Title,
      Body = note.BodyPlainText
    };
    await SearchService.WriteNoteIndexAsync(searchEntity);

    return note;
  }

  public async Task<bool> DeleteNotePermanentlyAsync(NoteId noteId)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync();
    var entity = await context.NoteEntities.FirstOrDefaultAsync(e => e.Id == noteId.Value);
    if (entity is not null)
    {
      context.NoteEntities.Remove(entity);
      await SearchService.DeleteNoteIndexAsync(noteId.Value);
      return await context.SaveChangesAsync() > 0;
    }
    return false;
  }
}