using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Notes;

namespace MyNotes.ViewModels.Notes.Providers;

internal sealed class NoteViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<Note, NoteViewModel>
{
  private readonly ConcurrentDictionary<Note, ReferenceCounter<NoteViewModel>> ResolveTable = new();

  public NoteViewModel Resolve(Note note)
  {
    var rc = ResolveTable.GetOrAdd(note, n => new ReferenceCounter<NoteViewModel>()
    {
      Instance = ActivatorUtilities.CreateInstance<NoteViewModel>(serviceProvider, n)
    });

    rc.Increment();
    return rc.Instance;
  }

  public bool TryResolve(Note note, [NotNullWhen(true)] out NoteViewModel? noteViewModel)
  {
    if (ResolveTable.TryGetValue(note, out var rc))
    {
      var viewmodel = rc.Instance;
      if (!rc.HasNoReferences && !viewmodel.Disposed)
      {
        noteViewModel = viewmodel;
        return true;
      }
      else
      {
        ResolveTable.TryRemove(note, out _);
      }
    }
    noteViewModel = null;
    return false;
  }

  public bool Release(Note note)
  {
    if (ResolveTable.TryGetValue(note, out var rc) && rc.Decrement())
    {
      ResolveTable.TryRemove(note, out _);
      return true;
    }
    return false;
  }
}