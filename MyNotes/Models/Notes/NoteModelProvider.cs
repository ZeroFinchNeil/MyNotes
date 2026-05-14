using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using MyNotes.Domain.ValueObjects;

namespace MyNotes.Models.Notes;

internal class NoteModelProvider : IModelProvider<NoteId, NoteModel>
{
  private readonly ConcurrentDictionary<NoteId, NoteModel> ResolveTable = new();

  public NoteModel Resolve(NoteId key, Func<NoteModel> noteModelFactory) => ResolveTable.GetOrAdd(key, _ => noteModelFactory.Invoke());

  public bool TryResolve(NoteId key, [NotNullWhen(true)] out NoteModel? noteModel)
  {
    if (ResolveTable.TryGetValue(key, out var model))
    {
      noteModel = model;
      return true;
    }
    noteModel = null;
    return false;
  }

  public bool Release(NoteId key) => ResolveTable.TryRemove(key, out _);
}
