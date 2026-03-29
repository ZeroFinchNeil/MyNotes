using System.Text.Json;

using CommunityToolkit.WinUI.Helpers;

using Microsoft.EntityFrameworkCore;

using MyNotes.AppConstants;
using MyNotes.Common.Interop;
using MyNotes.Helpers;
using MyNotes.Models.Media;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.App;
using MyNotes.Services.Database;
using MyNotes.Services.Database.Entities;
using MyNotes.Services.Search;
using MyNotes.Services.Search.Entities;
using MyNotes.Services.Settings;
using MyNotes.ViewModels.Media;
using MyNotes.ViewModels.Media.Providers;
using MyNotes.Views.Windows;

namespace MyNotes.Services.Notes;

internal sealed partial class NoteService : IDisposable
{
  private readonly IDbContextFactory<AppDbContext> DbContextFactory;
  private readonly SettingsService SettingsService;
  private readonly WindowService WindowService;
  private readonly SearchService SearchService;
  private readonly ImageViewModelProvider ImageViewModelProvider;

  private readonly TaskCompletionSource InitializationTCS = new();
  public Task InitializationTask => InitializationTCS.Task;

  #region Object Lifetime Management
  public NoteService(IDbContextFactory<AppDbContext> dbContextFactory, SettingsService settingsService, WindowService windowService, SearchService searchService, ImageViewModelProvider imageViewModelProvider)
  {
    // DI
    DbContextFactory = dbContextFactory;
    SettingsService = settingsService;
    WindowService = windowService;
    SearchService = searchService;
    ImageViewModelProvider = imageViewModelProvider;

    _ = InitializeAsync();
  }

  private async Task InitializeAsync()
  {
    await using var context = await DbContextFactory.CreateDbContextAsync();
    var entities = context.NoteEntities.Where(e => e.Parent == NavigationId.Empty.Value);
    foreach (var id in entities.Select(e => e.Id))
    {
      await SearchService.DeleteNoteIndexAsync(id);
    }
    context.NoteEntities.RemoveRange(entities);

    InitializationTCS.TrySetResult();
  }

  public bool Disposed { get; private set; }

  public void Dispose()
  {
    if (Disposed)
      return;

    Disposed = true;
  }
  #endregion

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
      WindowService.NoteWindowTable.TryGetValue(note.Id, out var wr)
      && wr.TryGetTarget(out var existingNoteWindow)
      && !existingNoteWindow.IsClosed
      ? existingNoteWindow
      : new(note);

    if (activate)
    {
      await noteWindow.LoadTask;
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

  public Task CommitSearchIndexAsync() => SearchService.CommitAsync();
}

#region Create (Add)
partial class NoteService
{
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

    Note note = CreateDefaultNote(noteId, navigationId);

    if (WindowService.TryGetFocusedWindow(out var focusedWindow, out var hWnd)
      && NativeMethods.GetMonitorInfoForWindow(hWnd) is NativeMethods.MONITORINFOEX monitorInfo)
    {
      var rect = monitorInfo.rcWork;
      int monitorWidth = rect.Right - rect.Left;
      int monitorHeight = rect.Bottom - rect.Top;
      int padding = 10;
      Range horizontal = new(rect.Left + padding, rect.Left + (monitorWidth - note.Size.Width) / 2);
      Range vertical = new(rect.Top + padding, rect.Top + (monitorHeight - note.Size.Height) / 2);

      Random random = new();
      int positionX = horizontal.Start.Value < horizontal.End.Value ? random.Next(horizontal.Start.Value, horizontal.End.Value) : horizontal.Start.Value;
      int positionY = vertical.Start.Value < vertical.End.Value ? random.Next(vertical.Start.Value, vertical.End.Value) : vertical.Start.Value;

      note.Position = new PointInt32(positionX, positionY);
    }

    NoteDbContextEntity entity = NoteToDbContextEntity(note);

    context.NoteEntities.Add(entity);
    await context.SaveChangesAsync();

    NoteSearchEntity searchEntity = NoteToSearchEntity(note);
    await SearchService.WriteNoteIndexAsync(searchEntity);

    return note;
  }
}
#endregion

#region Read (Get and Find)
partial class NoteService
{
  /// <summary>
  /// 지정한 NoteId에 해당하는 노트를 데이터베이스에서 비동기적으로 검색합니다.
  /// </summary>
  /// <param name="noteId">검색하려는 노트의 NoteId입니다.</param>
  public async Task<Note?> FindNoteAsync(NoteId noteId)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync();
    return (await context.NoteEntities.FindAsync(noteId.Value)) is NoteDbContextEntity e ? DbContextEntityToNote(e) : null;
  }

  /// <summary>
  /// 입력한 조건에 맞는 모든 노트들을 데이터베이스에서 비동기적으로 검색합니다.
  /// </summary>
  /// <param name="predicate">NoteEntity가 원하는 조건에 해당하면 true를 반환하는 predicate입니다.</param>
  public async Task<IReadOnlyList<Note>> GetNotesAsync(Func<NoteDbContextEntity, bool> predicate)
  {
    List<Note> notes;

    await using (var context = await DbContextFactory.CreateDbContextAsync())
    {
      notes = [.. context.NoteEntities
        .Where(predicate)
        .Select(DbContextEntityToNote)];
    }
    return notes;
  }
}
#endregion

#region Update
partial class NoteService
{
  /// <summary>
  /// <para>노트 엔티티를 주어진 액션에 따라 데이터베이스에 비동기 업데이트합니다. 데이터베이스에 일치하는 id를 가진 엔티티가 없으면 액션이 실행되지 않고 변경사항이 저장되지 않습니다.</para>
  /// <para>Asynchronously updates a note entity in the database by applying a specified action to it. If no entity with the specified note id exists, the action is not invoked and no changes are made.</para>
  /// </summary>
  /// <param name="action">
  /// <para>일치하는 노트 엔티티에서 수행해야 할 업데이트를 포함한 액션입니다.</para>
  /// <para>An action to perform on the found note entity.</para>
  /// </param>
  private async Task UpdateDbContextEntityAsync(Note note, Action<NoteDbContextEntity> action)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync();
    if (await context.NoteEntities.FindAsync(note.Id.Value) is NoteDbContextEntity entity)
    {
      action.Invoke(entity);
      await context.SaveChangesAsync();
    }
  }

  private Task UpdateSearchEntityAsync(Note note) => SearchService.WriteNoteIndexAsync(NoteToSearchEntity(note));

  public async Task UpdateNoteEntityAsync(Note note, IEnumerable<string> changedNoteProperties)
  {
    changedNoteProperties = changedNoteProperties.Distinct();
    Action<NoteDbContextEntity>? dbActions = null;
    bool _updateNoteSearchIndex = false;
    foreach (var propertyName in changedNoteProperties)
    {
      if (_notePropertyToDbContextEntityActions.TryGetValue(propertyName, out var dbAction))
      {
        dbActions += dbAction(note);
      }
      if (_notePropertyToNoteSearchEntity.Contains(propertyName))
        _updateNoteSearchIndex = true;
    }

    if (dbActions is not null)
    {
      await UpdateDbContextEntityAsync(note, dbActions);
    }

    if (_updateNoteSearchIndex)
    {
      await UpdateSearchEntityAsync(note);
    }
  }
}
#endregion

#region Delete
partial class NoteService
{
  private async Task<bool> DeleteDbContextEntityAsync(NoteId noteId)
  {
    await using var context = await DbContextFactory.CreateDbContextAsync();
    var entity = await context.NoteEntities.FirstOrDefaultAsync(e => e.Id == noteId.Value);
    if (entity is not null)
    {
      context.NoteEntities.Remove(entity);
      return await context.SaveChangesAsync() > 0;
    }
    return false;
  }

  private Task DeleteSearchEntityAsync(NoteId noteId) => SearchService.DeleteNoteIndexAsync(noteId.Value);

  public async Task<bool> DeleteNotePermanentlyAsync(NoteId noteId)
  {
    await DeleteSearchEntityAsync(noteId);
    return await DeleteDbContextEntityAsync(noteId);
  }
}
#endregion

#region Cache and Mapper
partial class NoteService
{
  private readonly Dictionary<NoteId, WeakReference<Note>> NoteCache = new();

  /// <summary>
  /// <para>노트 속성과 데이터베이스 노트 엔티티의 해당 속성을 업데이트하는 작업 간의 매핑을 제공합니다. 이 딕셔너리는 'Note' 객체와 데이터베이스의 'NoteEntity' 표현 간의 효율적인 동기화를 가능하게 합니다. 각 항목은 'Note' 클래스의 속성 이름과 해당 'Note' 객체가 주어졌을 때 'NoteEntity'의 관련 속성을 업데이트하는 작업을 반환하는 함수를 연결합니다. 이 매핑은 불변이므로 스레드 안전성을 보장하고 의도치 않은 수정을 방지합니다.</para>
  /// <para>Provides a mapping of note property names to actions that update corresponding properties on a database entity. This dictionary enables efficient synchronization between 'Note' objects and their associated 'NoteEntity' representations in the database. Each entry associates a property name from the 'Note' class with a  function that, given a 'Note', returns an action to update the relevant property on a 'NoteEntity'. The mapping is  immutable, ensuring thread safety and preventing accidental modification.</para>
  /// </summary>
  private static readonly ImmutableDictionary<string, Func<Note, Action<NoteDbContextEntity>>> _notePropertyToDbContextEntityActions = ImmutableDictionary.CreateRange(new Dictionary<string, Func<Note, Action<NoteDbContextEntity>>>()
  {
    { nameof(Note.NavigationId), note => e => e.Parent = note.NavigationId.Value },
    { nameof(Note.Modified), note => e => e.Modified = note.Modified },
    { nameof(Note.Title), note => e => e.Title = note.Title },
    { nameof(Note.Body), note => e => e.Body = note.Body },
    { nameof(Note.BackgroundColor), note => e => e.BackgroundColor = note.BackgroundColor.ToString() },
    { nameof(Note.ShowBackgroundImage), note => e => e.ShowBackgroundImage = note.ShowBackgroundImage },
    { nameof(Note.BackgroundImagePath), note => e => e.BackgroundImagePath = note.BackgroundImagePath },
    { nameof(Note.BackgroundImageOpacity), note => e => e.BackgroundImageOpacity = note.BackgroundImageOpacity },
    { nameof(Note.BackgroundImageBlur), note => e => e.BackgroundImageBlur = note.BackgroundImageBlur },
    { nameof(Note.BackdropKind), note => e => e.BackdropKind = (int)note.BackdropKind },
    { nameof(Note.BackdropTintOpacity), note => e => e.BackdropTintOpacity = Math.Round(note.BackdropTintOpacity, 2) },
    { nameof(Note.BackdropLuminosityOpacity), note => e => e.BackdropLuminosityOpacity = Math.Round(note.BackdropLuminosityOpacity, 2) },
    { nameof(Note.Images), note => e => e.Images = JsonSerializer.Serialize(note.Images, AppJson.JsonSerializerOptions) },
    { nameof(Note.ShowImagePanel), note => e => e.ShowImagePanel = note.ShowImagePanel },
    { nameof(Note.ImagePanelHeight), note => e => e.ImagePanelHeight = Math.Round(note.ImagePanelHeight, 2) },
    { nameof(Note.Size), note => e =>
      {
        e.Width = note.Size.Width;
        e.Height = note.Size.Height;
      }
    },
    { nameof(Note.Position), note => e =>
      {
        e.PositionX = note.Position.X;
        e.PositionY = note.Position.Y;
      }
    },
    { nameof(Note.IsBookmarked), note => e => e.IsBookmarked = note.IsBookmarked },
    { nameof(Note.IsDeleted), note => e => e.IsDeleted = note.IsDeleted },
    { nameof(Note.IsWindowOpen), note => e => e.IsWindowOpen = note.IsWindowOpen },
    { nameof(Note.IsAlwaysOnTop), note => e => e.IsAlwaysOnTop = note.IsAlwaysOnTop }
  });
  private static readonly ImmutableHashSet<string> _notePropertyToNoteSearchEntity = [nameof(Note.Title), nameof(Note.BodyPlainText)];

  private Note DbContextEntityToNote(NoteDbContextEntity e)
  {
    NoteId noteId = NoteId.Create(e.Id);
    if (NoteCache.TryGetValue(noteId, out var wr)
        && wr.TryGetTarget(out var existingNote))
    {
      return existingNote;
    }

    List<ImageDescriptor>? images = null;
    try
    {
      images = JsonSerializer.Deserialize<List<ImageDescriptor>>(e.Images);
    }
    catch
    { }
    images ??= new();

    Note note = new()
    {
      Id = noteId,
      NavigationId = NavigationId.GetOrCreate(e.Parent),
      Created = e.Created,
      Title = e.Title,
      Body = e.Body,
      BackgroundColor = e.BackgroundColor.ToColor(),
      ShowBackgroundImage = e.ShowBackgroundImage,
      BackgroundImagePath = e.BackgroundImagePath,
      BackgroundImageOpacity = e.BackgroundImageOpacity,
      BackgroundImageBlur = e.BackgroundImageBlur,
      BackdropKind = (BackdropKind)e.BackdropKind,
      BackdropTintOpacity = e.BackdropTintOpacity,
      BackdropLuminosityOpacity = e.BackdropLuminosityOpacity,
      Images = [.. images],
      ShowImagePanel = e.ShowImagePanel,
      ImagePanelHeight = e.ImagePanelHeight,
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

  private static NoteDbContextEntity NoteToDbContextEntity(Note note) => new()
  {
    Id = note.Id.Value,
    Parent = note.NavigationId.Value,
    Created = note.Created,
    Modified = note.Modified,
    Title = note.Title,
    Body = note.Body,
    BackgroundColor = note.BackgroundColor.ToString(),
    ShowBackgroundImage = note.ShowBackgroundImage,
    BackgroundImagePath = note.BackgroundImagePath,
    BackgroundImageOpacity = note.BackgroundImageOpacity,
    BackgroundImageBlur = note.BackgroundImageBlur,
    BackdropKind = (int)note.BackdropKind,
    BackdropTintOpacity = note.BackdropTintOpacity,
    BackdropLuminosityOpacity = note.BackdropLuminosityOpacity,
    Images = JsonSerializer.Serialize(note.Images, AppJson.JsonSerializerOptions),
    ShowImagePanel = note.ShowImagePanel,
    ImagePanelHeight = note.ImagePanelHeight,
    Width = note.Size.Width,
    Height = note.Size.Height,
    PositionX = note.Position.X,
    PositionY = note.Position.Y,
    IsBookmarked = note.IsBookmarked,
    IsDeleted = note.IsDeleted,
    IsWindowOpen = note.IsWindowOpen,
    IsAlwaysOnTop = note.IsAlwaysOnTop
  };

  private static NoteSearchEntity NoteToSearchEntity(Note note) => new()
  {
    Id = note.Id.Value,
    Title = note.Title,
    Body = note.BodyPlainText
  };

  private Note CreateDefaultNote(NoteId noteId, NavigationId navigationId) => new()
  {
    Id = noteId,
    NavigationId = navigationId,
    Created = DateTimeOffset.UtcNow,
    BackgroundColor = SettingsService.Load(AppSettingsDescriptors.NoteBackground).ToColor(),
    ShowBackgroundImage = false,
    BackgroundImagePath = null,
    BackgroundImageOpacity = 1.0,
    BackgroundImageBlur = 0.0,
    BackdropKind = (BackdropKind)SettingsService.Load(AppSettingsDescriptors.NoteBackdropKind),
    BackdropTintOpacity = 0.5,
    BackdropLuminosityOpacity = 0.0,
    Images = [],
    ShowImagePanel = true,
    ImagePanelHeight = 180.0,
    Size = SettingsService.Load(AppSettingsDescriptors.NoteSize).SizeInt32,
    IsBookmarked = false,
    IsDeleted = false,
    IsWindowOpen = false,
    IsAlwaysOnTop = false,
  };

  public ImageCollectionKey CreateImageCollectionKey(Note note) => new()
  {
    Key = note.Id.Value,
    CollectionReference = new WeakReference<ObservableCollection<ImageViewModel>>([.. note.Images.Select(ImageViewModelProvider.Resolve)])
  };
}
#endregion