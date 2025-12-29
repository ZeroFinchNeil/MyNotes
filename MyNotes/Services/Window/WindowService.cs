using MyNotes.Debugging;
using MyNotes.Models.Notes;
using MyNotes.Views.Windows;

namespace MyNotes.Services.Window;

internal sealed class WindowService
{
  public WeakReference<MainWindow>? MainWindow;

  public Dictionary<Note, WeakReference<NoteWindow>> NoteWindows { get; } = new();

  public Dictionary<Guid, WeakReference<BlankWindow>> BlankWindows { get; } = new();
}