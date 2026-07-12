using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using MyNotes.Domain.ValueObjects;

namespace MyNotes.Models.Notes;

internal sealed class NoteModelStore : IModelStore<NoteId, NoteModel>
{
  private readonly ConcurrentDictionary<NoteId, WeakReference<NoteModel>> ResolveTable = new();

  public NoteModel GetOrAdd(NoteId key, Func<NoteId, NoteModel> factory)
  {
    throw new NotImplementedException();
  }

  public NoteModel AddOrUpdate(NoteId key, Func<NoteId, NoteModel> factory, Action<NoteModel> updater)
  {
    if (ResolveTable.TryGetValue(key, out var wr) && wr.TryGetTarget(out var model))
    {
      updater(model);
      return model;
    }

    ResolveTable.TryRemove(key, out _);
    NoteModel noteModel = factory(key);
    ResolveTable.TryAdd(key, new(noteModel));

    return noteModel;
  }

  public bool TryGetModel(NoteId key, [NotNullWhen(true)] out NoteModel? noteModel)
  {
    if (ResolveTable.TryGetValue(key, out var wr) && wr.TryGetTarget(out var model))
    {
      noteModel = model;
      return true;
    }

    ResolveTable.TryRemove(key, out _);

    noteModel = null;
    return false;
  }

  public bool Release(NoteId key) => ResolveTable.TryRemove(key, out _);

}
