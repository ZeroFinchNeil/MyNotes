using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Lifetime;
using MyNotes.Domain.Notes;
using MyNotes.Models.Notes;

namespace MyNotes.ViewModels.Notes.Providers;

internal sealed class NoteViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<NoteModel, NoteViewModel>
{
  private readonly ConcurrentDictionary<NoteId, ReferenceCounter<NoteViewModel>> ResolveTable = new();
  private readonly Func<NoteModel, NoteViewModel> _factory = noteModel => ActivatorUtilities.CreateInstance<NoteViewModel>(serviceProvider, noteModel);

  public NoteViewModel Resolve(NoteModel noteModel)
  {
    var counter = ResolveTable.GetOrAdd(noteModel.Id, noteId => new ReferenceCounter<NoteViewModel>(_factory(noteModel)));

    if (counter.TryAcquire(out var viewmodel))
    {
      return viewmodel;
    }

    var newCounter = new ReferenceCounter<NoteViewModel>(_factory(noteModel));
    ResolveTable.AddOrUpdate(noteModel.Id, newCounter, (k, v) => v = newCounter);

    return newCounter.TryAcquire(out var newViewModel) ? newViewModel : throw new InvalidOperationException();
  }

  public bool TryResolve(NoteModel noteModel, [NotNullWhen(true)] out NoteViewModel? noteViewModel)
  {
    NoteId noteId = noteModel.Id;
    if (ResolveTable.TryGetValue(noteId, out var counter)
      && counter.TryAcquire(out var viewmodel, false))
    {
      if (!viewmodel.Disposed)
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

  public bool Release(NoteModel noteModel)
  {
    if (ResolveTable.TryGetValue(noteModel.Id, out var counter))
    {
      var viewmodel = counter.Release();
      viewmodel.Dispose();
      return true;
    }

    return false;
  }
}