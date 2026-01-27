using System.Runtime.CompilerServices;

using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Notes;
using MyNotes.Views.Navigations;
using MyNotes.Views.Notes;
using MyNotes.Views.Windows;

namespace MyNotes.Debugging;

internal static class ReferenceTracker
{
  public static ConditionalWeakTable<Window, object?> WindowReference = new();
  public static ConditionalWeakTable<Page, object?> PageReference = new();
  public static ConditionalWeakTable<ViewModelBase, object?> ViewModelReference = new();

  public static ConditionalWeakTable<INavigation, object?> NavigationReference = new();
  public static ConditionalWeakTable<Note, object?> NoteReference = new();

  public static ConditionalWeakTable<FrameworkElement, object?> ElementReference = new();
}
