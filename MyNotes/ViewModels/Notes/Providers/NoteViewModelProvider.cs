using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Debugging;
using MyNotes.Domain.Notes;
using MyNotes.Models.Notes;

namespace MyNotes.ViewModels.Notes.Providers;

internal sealed class NoteViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<NoteModel, NoteViewModel>
{
  private readonly ConcurrentDictionary<NoteId, ReferenceCounter<NoteViewModel>> ResolveTable = new();

  public NoteViewModel Resolve(NoteModel noteModel)
  {
    var rc = ResolveTable.GetOrAdd(noteModel.Id, noteId => new ReferenceCounter<NoteViewModel>()
    {
      Instance = ActivatorUtilities.CreateInstance<NoteViewModel>(serviceProvider, noteModel)
    });

    rc.Increment();
    return rc.Instance;
  }

  public bool TryResolve(NoteModel noteModel, [NotNullWhen(true)] out NoteViewModel? noteViewModel)
  {
    NoteId noteId = noteModel.Id;
    if (ResolveTable.TryGetValue(noteId, out var rc))
    {
      var viewmodel = rc.Instance;
      if (!rc.HasNoReferences && !viewmodel.Disposed)
      {
        noteViewModel = viewmodel;
        return true;
      }
      else
      {
        ResolveTable.TryRemove(noteId, out _);
      }
    }
    noteViewModel = null;
    return false;
  }

  public bool Release(NoteModel noteModel) =>
    ResolveTable.TryGetValue(noteModel.Id, out var rc) && rc.Decrement() && ResolveTable.TryRemove(noteModel.Id, out _);
}