using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Notes;

namespace MyNotes.ViewModels.Notes;

internal sealed class NoteViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<Note, NoteViewModel>
{
  private readonly IServiceProvider ServiceProvider = serviceProvider;

  private readonly Dictionary<Note, WeakReference<NoteViewModel>> ResolvedViewModels = new();

  public NoteViewModel Resolve(Note note)
  {
    if (ResolvedViewModels.TryGetValue(note, out var wr)
      && wr.TryGetTarget(out var viewmodel))
    {
      Console.WriteLine("{0}: {1}", "Existing", note.Id.Value);
      return viewmodel;
    }

    NoteViewModel newViewModel = ActivatorUtilities.CreateInstance<NoteViewModel>(ServiceProvider, note);
    ResolvedViewModels[note] = new WeakReference<NoteViewModel>(newViewModel);

    return newViewModel;
  }

  public bool TryResolve(Note note, out NoteViewModel? noteViewModel)
  {
    if (ResolvedViewModels.TryGetValue(note, out var wr)
      && wr.TryGetTarget(out var viewmodel))
    {
      noteViewModel = viewmodel;
      return true;
    }

    noteViewModel = null;
    return false;
  }
}
