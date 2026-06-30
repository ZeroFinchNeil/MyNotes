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