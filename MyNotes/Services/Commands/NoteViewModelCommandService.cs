using MyNotes.Common.Commands;
using MyNotes.Common.Structures;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.Notes;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Notes;

namespace MyNotes.Services.Commands;

internal sealed class NoteViewModelCommandService : ICommandService
{
  private readonly NoteService NoteService;
  private readonly NavigationViewModelProvider NavigationViewModelProvider;

  public Command<NoteViewModel> OpenWindowCommand { get; private set; }
  public Command<SourceTargetPair<NoteViewModel, NavigationId>> MoveToListCommand { get; private set; }

  public NoteViewModelCommandService(NoteService noteService, NavigationViewModelProvider navigationViewModelProvider)
  {
    NoteService = noteService;
    NavigationViewModelProvider = navigationViewModelProvider;

    OpenWindowCommand = new(
      actionToExecute: (noteViewModel) =>
      {
        NoteService.OpenNoteWindow(noteViewModel.Note);
      }
    );

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
            && s is UserLeafNavigationViewModel sourceViewModel)
        {
          sourceViewModel.NoteViewModels?.Remove(sourceNoteViewModel);
        }
      });
  }
}
