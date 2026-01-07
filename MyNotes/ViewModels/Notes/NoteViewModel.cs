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

    Note.PropertyChanged += Note_PropertyChanged;
  }

  private void Note_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if(e.PropertyName == nameof(Note.Backdrop))
    {
      this.Backdrop = (int)Note.Backdrop;
    }
  }

  public int Backdrop
  {
    get;
    set
    {
      if (field != value)
      {
        SetProperty(ref field, value);
        Note.Backdrop = (BackdropKind)value;
      }
    }
  }

  protected override void Dispose(bool disposing)
  {
    if (_disposed)
      return;

    if (disposing)
    {
      Note.PropertyChanged -= Note_PropertyChanged;
    }

    _disposed = true;
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
