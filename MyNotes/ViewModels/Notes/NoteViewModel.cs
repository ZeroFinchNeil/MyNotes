using MyNotes.Common.Commands;
using MyNotes.Debugging;
using MyNotes.Models.Notes;

namespace MyNotes.ViewModels.Notes;

internal sealed partial class NoteViewModel : ViewModelBase
{
  public Note Note { get; }
  public NoteViewModel(Note note)
  {
#if DEBUG
    ReferenceTracker.NoteViewModelReference.Add(this, note.Id.Value);
#endif
    Note = note;
    SetCommand();
  }
}

internal sealed partial class NoteViewModel : ViewModelBase
{
  public Command? SaveCommand { get; private set; }

  private void SetCommand()
  {
    SaveCommand = new(
      actionToExecute: () =>
      {
        Console.WriteLine("{0}: {1}", "Save Note.", "");
      });
  }
}
