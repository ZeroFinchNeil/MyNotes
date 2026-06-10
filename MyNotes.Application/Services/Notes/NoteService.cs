using MyNotes.Application.Contracts.Database.Repositories.Notes;
using MyNotes.Domain.ValueObjects;
using MyNotes.Services.Settings;

namespace MyNotes.Application.Services.Notes;

internal sealed partial class NoteService : IDisposable
{
  private readonly INoteRepository NoteRepository;
  private readonly SettingsService SettingsService;

  public NoteCreationService Creation { get; }
  public NoteRetrievalService Retrieval { get; }
  public NoteModificationService Modification { get; }

  private readonly TaskCompletionSource InitializationTCS = new();
  public Task InitializationTask => InitializationTCS.Task;

  #region Object Lifetime Management
  public NoteService(INoteRepository noteRepository, SettingsService settingsService, NoteCreationService noteCreationService, NoteRetrievalService noteRetrievalService, NoteModificationService noteModificationService)
  {
    // DI
    NoteRepository = noteRepository;
    SettingsService = settingsService;

    Creation = noteCreationService;
    Retrieval = noteRetrievalService;
    Modification = noteModificationService;

    _ = InitializeAsync();
  }

  private async Task InitializeAsync()
  {
#if false
    await using var context = await DbContextFactory.CreateDbContextAsync();
    var entities = context.NoteEntities.Where(e => e.Parent == NavigationId.Empty.Value);
    foreach (var id in entities.Select(e => e.Id))
    {
      await SearchService.DeleteNoteIndexAsync(id);
    }
    context.NoteEntities.RemoveRange(entities);
#endif

    InitializationTCS.TrySetResult();
  }

  public bool Disposed { get; private set; }

  public void Dispose()
  {
    if (Disposed)
    {
      return;
    }

    Disposed = true;
  }
  #endregion

  public Task<NoteId> GetUniqueNoteIdAsync() => NoteRepository.GenerateUniqueNoteIdAsync();

  public async Task<int> OpenNoteWindowsForOpenEntities()
  {
#if false
    int result = 0;
    foreach (var note in await GetNotesAsync(e => e.IsWindowOpen))
    {
      await OpenNoteWindow(note);
      result++;
    }
    return result;
#endif
    return 0;
  }

  public Task CommitSearchIndexAsync() => throw new NotImplementedException();
}

#if false
#region Create (Add)
partial class NoteService
{
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
      {
        _updateNoteSearchIndex = true;
      }
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
}
#endregion
#endif