using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Notes;

namespace MyNotes.ViewModels.Notes;

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
        && !viewmodel.IsDisposed)
    {
      noteViewModel = viewmodel;
      return true;
    }

    noteViewModel = null;
    return false;
  }
}
