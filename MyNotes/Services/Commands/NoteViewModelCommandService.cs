using MyNotes.Common.Commands;
using MyNotes.Common.Structures;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.Notes;
using MyNotes.Services.Windows;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Notes;

namespace MyNotes.Services.Commands;

internal sealed class NoteViewModelCommandService : ICommandService
{
  private readonly NoteService NoteService;
  private readonly NavigationViewModelProvider NavigationViewModelProvider;
  private readonly NoteViewModelProvider NoteViewModelProvider;
  private readonly NoteListViewModelProvider NoteListViewModelProvider;
  private readonly WindowService WindowService;

  public Command<NoteViewModel> OpenWindowCommand { get; }
  public Command<SourceTargetPair<NoteViewModel, NavigationId>> MoveToListCommand { get; }
  public Command<NoteViewModel> CreateNewNoteCommand { get; }
  public Command<NoteViewModel> ViewListCommand { get; }

  public NoteViewModelCommandService(NoteService noteService, NavigationViewModelProvider navigationViewModelProvider, NoteViewModelProvider noteViewModelProvider, NoteListViewModelProvider noteListViewModelProvider, WindowService windowService)
  {
    NoteService = noteService;
    NavigationViewModelProvider = navigationViewModelProvider;
    NoteViewModelProvider = noteViewModelProvider;
    NoteListViewModelProvider = noteListViewModelProvider;
    WindowService = windowService;

    OpenWindowCommand = new(
      actionToExecute: (noteViewModel) =>
      {
        NoteService.OpenNoteWindow(noteViewModel.Note);
      });

    MoveToListCommand = new(
      actionToExecute: async (pair) =>
      {
        NoteViewModel sourceNoteViewModel = pair.Source;
        Note sourceNote = sourceNoteViewModel.Note;
        NavigationId oldNavigationId = sourceNote.NavigationId;
        NavigationId newNavigationId = pair.Target;

        if (sourceNote.NavigationId == newNavigationId)
          return;

        sourceNote.NavigationId = newNavigationId;
        await NoteService.UpdateNoteEntityAsync(sourceNote, entity => entity.Parent = newNavigationId.Value);

        if (NavigationViewModelProvider.TryResolve(oldNavigationId, out var s)
            && s is UserLeafNavigationViewModel sourceViewModel
            && NoteListViewModelProvider.TryResolve(sourceViewModel.Navigation, out var noteListViewModel))
        {
          noteListViewModel?.NoteViewModels?.Remove(sourceNoteViewModel);
        }
      });

    CreateNewNoteCommand = new(
      actionToExecute: async (noteViewModel) =>
      {
        if (NavigationViewModelProvider.TryResolve(noteViewModel.Note.NavigationId, out var nvm)
            && nvm is UserLeafNavigationViewModel navigationViewModel)
        {
          if (await NoteService.AddNoteAsync(navigationViewModel.Navigation) is Note newNote)
          {
            NoteViewModel newNoteViewModel = NoteViewModelProvider.Resolve(newNote);
            NoteService.OpenNoteWindow(newNote);

            if (NoteListViewModelProvider.TryResolve(navigationViewModel.Navigation, out var noteListViewModel))
            {
              noteListViewModel.NoteViewModels?.Add(newNoteViewModel);
            }
          }
        }
      });

    ViewListCommand = new(
      actionToExecute: (noteViewModel) =>
      {
        var mainWindow = WindowService.GetOrCreateMainWindow(noteViewModel.Note.NavigationId);
        mainWindow.Activate();
      });
  }
}
