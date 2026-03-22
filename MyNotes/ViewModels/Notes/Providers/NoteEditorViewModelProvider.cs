using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Notes;

namespace MyNotes.ViewModels.Notes.Providers;

internal sealed class NoteEditorViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<Note, NoteEditorViewModel>
{
  private readonly IServiceProvider ServiceProvider = serviceProvider;

  private readonly Dictionary<Note, WeakReference<NoteEditorViewModel>> ResolvedViewModels = new();

  NoteEditorViewModel IViewModelProvider<Note, NoteEditorViewModel>.Resolve(Note note) => throw new NotImplementedException();

  public NoteEditorViewModel Resolve(Note note, RichEditTextDocument document)
  {
    if (TryResolve(note, out var viewmodel))
    {
      return viewmodel;
    }

    NoteEditorViewModel newViewModel = ActivatorUtilities.CreateInstance<NoteEditorViewModel>(ServiceProvider, note, document);
    ResolvedViewModels[note] = new WeakReference<NoteEditorViewModel>(newViewModel);

    return newViewModel;
  }

  public bool TryResolve(Note note, [NotNullWhen(true)] out NoteEditorViewModel? noteEditorViewModel)
  {
    if (ResolvedViewModels.TryGetValue(note, out var wr)
        && wr.TryGetTarget(out var viewmodel)
        && !viewmodel.Disposed)
    {
      noteEditorViewModel = viewmodel;
      return true;
    }

    noteEditorViewModel = null;
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
