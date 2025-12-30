using System.Runtime.CompilerServices;

using MyNotes.Views.Navigations;
using MyNotes.Views.Notes;
using MyNotes.Views.Windows;

namespace MyNotes.Debugging;

internal static class ReferenceTracker
{
  public static ConditionalWeakTable<BlankWindow, object?> BlankWindowReference = new();
  public static ConditionalWeakTable<MainWindow, object?> MainWindowReference = new();
  public static ConditionalWeakTable<NoteWindow, object?> NoteWindowReference = new();
  public static ConditionalWeakTable<MainPage, object?> MainPageReference = new();
  public static ConditionalWeakTable<NotePage, object?> NotePageReference = new();
}
