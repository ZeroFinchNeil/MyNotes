using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using MyNotes.Domain.ValueObjects;
using MyNotes.Infrastructure.Mappers;

namespace MyNotes.Models.Notes;

internal sealed class NoteModelStore : IModelStore<NoteId, NoteModel>
{
  private readonly ConcurrentDictionary<NoteId, WeakReference<NoteModel>> ResolveTable = new();

  public NoteModel GetOrAdd(NoteId key, Func<NoteId, NoteModel> factory)
  {
    throw new NotImplementedException();
  }

  public NoteModel Upsert<TSource>(NoteId key, Func<NoteId, NoteModel> factory, Action<NoteModel, TSource> updater)
  {
    throw new NotImplementedException();
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
