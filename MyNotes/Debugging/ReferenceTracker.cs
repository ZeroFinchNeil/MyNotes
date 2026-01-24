using System.Runtime.CompilerServices;

using MyNotes.ViewModels;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Notes;
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
  public static ConditionalWeakTable<UserListPage, object?> UserListPageReference = new();
  public static ConditionalWeakTable<SearchResultsPage, object?> SearchResultsPageReference = new();
  public static ConditionalWeakTable<NotePage, object?> NotePageReference = new();

  public static ConditionalWeakTable<MainViewModel, object?> MainViewModelReference = new();
  public static ConditionalWeakTable<NoteViewModel, object?> NoteViewModelReference = new();
  public static ConditionalWeakTable<NavigationViewModelBase, object?> NavigationViewModelReference = new();
}
