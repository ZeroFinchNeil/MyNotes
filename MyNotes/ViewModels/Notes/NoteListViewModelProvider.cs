using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Navigations;

namespace MyNotes.ViewModels.Notes;

internal class NoteListViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<INavigationNoteList, NoteListViewModel>
{
  private readonly IServiceProvider ServiceProvider = serviceProvider;

  private readonly Dictionary<INavigationNoteList, WeakReference<NoteListViewModel>> ResolvedViewModels = new();

  public NoteListViewModel Resolve(INavigationNoteList navigation)
  {
    if (ResolvedViewModels.TryGetValue(navigation, out var wr)
      && wr.TryGetTarget(out var viewmodel)
      && !viewmodel.IsDisposed)
    {
      return viewmodel;
    }

    NoteListViewModel newViewModel = ActivatorUtilities.CreateInstance<NoteListViewModel>(ServiceProvider, navigation);
    ResolvedViewModels[navigation] = new WeakReference<NoteListViewModel>(newViewModel);

    return newViewModel;
  }

  public bool TryResolve(INavigationNoteList navigation, out NoteListViewModel? noteViewModel)
  {
    if (ResolvedViewModels.TryGetValue(navigation, out var wr)
      && wr.TryGetTarget(out var viewmodel))
    {
      noteViewModel = viewmodel;
      return true;
    }

    noteViewModel = null;
    return false;
  }
}
