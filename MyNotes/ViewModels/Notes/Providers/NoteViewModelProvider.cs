using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Lifetime;
using MyNotes.Models.Notes;

namespace MyNotes.ViewModels.Notes.Providers;

internal sealed class NoteViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<NoteModel, NoteViewModel>
{
  private readonly ConcurrentDictionary<NoteModel, ReferenceCounter<NoteViewModel>> ResolveTable = new();

  public NoteViewModel Resolve(NoteModel noteModel)
  {
    var rc = ResolveTable.GetOrAdd(noteModel, n => new ReferenceCounter<NoteViewModel>()
    {
      Instance = ActivatorUtilities.CreateInstance<NoteViewModel>(serviceProvider, n)
    });

    rc.Increment();
    return rc.Instance;
  }

  public bool TryResolve(NoteModel noteModel, [NotNullWhen(true)] out NoteViewModel? noteViewModel)
  {
    if (ResolveTable.TryGetValue(noteModel, out var rc))
    {
      var viewmodel = rc.Instance;
      if (!rc.HasNoReferences && !viewmodel.Disposed)
      {
        noteViewModel = viewmodel;
        return true;
      }
      else
      {
        ResolveTable.TryRemove(noteModel, out _);
      }
    }
    noteViewModel = null;
    return false;
  }

  public bool Release(NoteModel noteModel) =>
    ResolveTable.TryGetValue(noteModel, out var rc) && rc.Decrement() && ResolveTable.TryRemove(noteModel, out _);
}