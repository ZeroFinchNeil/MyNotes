using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Notes;

namespace MyNotes.ViewModels.Notes.Providers;

internal sealed class NoteViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<Note, NoteViewModel>
{
  private readonly IServiceProvider ServiceProvider = serviceProvider;

  private readonly Dictionary<Note, WeakReference<NoteViewModel>> ResolvedViewModels = new();

  public NoteViewModel Resolve(Note note)
  {
    if (TryResolve(note, out var viewmodel))
    {
      return viewmodel;
    }

    NoteViewModel newViewModel = ActivatorUtilities.CreateInstance<NoteViewModel>(ServiceProvider, note);
    ResolvedViewModels[note] = new WeakReference<NoteViewModel>(newViewModel);

    return newViewModel;
  }

  public bool TryResolve(Note note, [NotNullWhen(true)] out NoteViewModel? noteViewModel)
  {
    if (ResolvedViewModels.TryGetValue(note, out var wr)
        && wr.TryGetTarget(out var viewmodel)
        && !viewmodel.Disposed)
    {
      noteViewModel = viewmodel;
      return true;
    }

    noteViewModel = null;
    return false;
  }

  public bool Release(Note note)
  {
    if (TryResolve(note, out var viewmodel))
    {
      if (!viewmodel.Disposed)
        viewmodel.Dispose();
      ResolvedViewModels.Remove(note);
    }
    return false;
  }

  public void ReleaseAll()
  {
    foreach (var wr in ResolvedViewModels.Values)
    {
      if (wr.TryGetTarget(out var viewmodel))
      {
        if (!viewmodel.Disposed)
          viewmodel.Dispose();
      }
    }
    ResolvedViewModels.Clear();
  }
}
