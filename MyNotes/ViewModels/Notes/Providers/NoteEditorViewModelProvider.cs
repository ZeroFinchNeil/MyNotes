using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Notes;

namespace MyNotes.ViewModels.Notes.Providers;

internal sealed class NoteEditorViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<NoteModel, NoteEditorViewModel>
{
  private readonly IServiceProvider ServiceProvider = serviceProvider;

  private readonly Dictionary<NoteModel, WeakReference<NoteEditorViewModel>> ResolvedViewModels = new();

  NoteEditorViewModel IViewModelProvider<NoteModel, NoteEditorViewModel>.Resolve(NoteModel note) => throw new NotImplementedException();

  public NoteEditorViewModel Resolve(NoteModel note, RichEditTextDocument document)
  {
    if (TryResolve(note, out var viewmodel))
    {
      return viewmodel;
    }

    NoteEditorViewModel newViewModel = ActivatorUtilities.CreateInstance<NoteEditorViewModel>(ServiceProvider, note, document);
    ResolvedViewModels[note] = new WeakReference<NoteEditorViewModel>(newViewModel);

    return newViewModel;
  }

  public bool TryResolve(NoteModel note, [NotNullWhen(true)] out NoteEditorViewModel? noteEditorViewModel)
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

  public bool Release(NoteModel note)
  {
    if (TryResolve(note, out var viewmodel))
    {
      if (!viewmodel.Disposed)
      {
        viewmodel.Dispose();
      }

      ResolvedViewModels.Remove(note);
    }
    return false;
  }
}
