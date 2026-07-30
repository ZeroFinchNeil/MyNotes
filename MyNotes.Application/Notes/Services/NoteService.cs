namespace MyNotes.Application.Notes.Services;

internal sealed partial class NoteService : IDisposable
{
  public NoteCreationService Creation { get; }
  public NoteRetrievalService Retrieval { get; }
  public NoteModificationService Modification { get; }

  private readonly TaskCompletionSource InitializationTCS = new();
  public Task InitializationTask => InitializationTCS.Task;

  #region Object Lifetime Management
  public NoteService(NoteCreationService noteCreationService, NoteRetrievalService noteRetrievalService, NoteModificationService noteModificationService)
  {
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
}