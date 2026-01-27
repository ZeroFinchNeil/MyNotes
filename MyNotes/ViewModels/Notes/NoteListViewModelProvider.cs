using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Navigations;

namespace MyNotes.ViewModels.Notes;

internal sealed class NoteListViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<INavigationNoteList, NoteListViewModel>
{
  private readonly IServiceProvider ServiceProvider = serviceProvider;

  private readonly Dictionary<INavigationNoteList, WeakReference<NoteListViewModel>> ResolvedViewModels = new();

  public NoteListViewModel Resolve(INavigationNoteList navigation)
  {
    if (TryResolve(navigation, out var viewmodel))
    {
      return viewmodel;
    }

    NoteListViewModel newViewModel = ActivatorUtilities.CreateInstance<NoteListViewModel>(ServiceProvider, navigation);
    ResolvedViewModels[navigation] = new WeakReference<NoteListViewModel>(newViewModel);

    return newViewModel;
  }

  public bool TryResolve(INavigationNoteList navigation, [NotNullWhen(true)] out NoteListViewModel? noteViewModel)
  {
    if (ResolvedViewModels.TryGetValue(navigation, out var wr)
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
