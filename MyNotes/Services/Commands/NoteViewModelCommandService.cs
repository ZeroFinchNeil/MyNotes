using MyNotes.Common.Commands;
using MyNotes.Services.Notes;
using MyNotes.ViewModels.Notes;

namespace MyNotes.Services.Commands;

internal sealed class NoteViewModelCommandService : ICommandService
{
  private readonly NoteService NoteService;

  public Command<NoteViewModel> OpenWindowCommand { get; private set; }

  public NoteViewModelCommandService(NoteService noteService)
  {
    NoteService = noteService;

    OpenWindowCommand = new(
      actionToExecute: (noteViewModel) =>
      {
        NoteService.OpenNoteWindow(noteViewModel.Note);
      }
    );
  }
}
