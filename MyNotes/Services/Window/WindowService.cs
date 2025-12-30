using MyNotes.Models.Notes;
using MyNotes.Views.Windows;

namespace MyNotes.Services.Window;

internal sealed class WindowService
{
  public WeakReference<MainWindow>? MainWindow;

  public Dictionary<NoteId, WeakReference<NoteWindow>> NoteWindows { get; } = new();
}