using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.Database;
using MyNotes.Services.Notes;
using MyNotes.Services.Window;
using MyNotes.Views.Windows;

namespace MyNotes.Debugging;

internal sealed partial class DebugWindow : Window
{
  public DebugWindow()
  {
    InitializeComponent();
    this.AppWindow.MoveAndResize(new(0, 0, 300, 300));
  }

  private async void DebugWindow_SeparatorButton_Click(object sender, RoutedEventArgs e)
  {
    Console.WriteLine();
    Console.WriteLine("--------------------");
    Console.WriteLine();
  }

  private async void DebugWindow_DebugButton_Click(object sender, RoutedEventArgs e)
  {
    WindowService windowService = App.Instance.Services.GetRequiredService<WindowService>();
    Console.WriteLine();
    Console.WriteLine("--------------------");
    PrintReference(ReferenceTracker.MainWindowReference, "Main Windows");
    PrintReference(ReferenceTracker.MainPageReference, "Main Pages");
    PrintReference(ReferenceTracker.MainViewModelReference, "Main ViewModels");
    PrintReference(ReferenceTracker.NavigationViewModelReference, "Navigation ViewModels");
    PrintReference(ReferenceTracker.UserListPageReference, "UserList Pages");
    PrintReference(ReferenceTracker.NoteWindowReference, "Note Windows");
    PrintReference(ReferenceTracker.NotePageReference, "Note Pages");
    PrintReference(ReferenceTracker.NoteViewModelReference, "Note ViewModels");
    PrintReference(ReferenceTracker.BlankWindowReference, "Blank Windows");
    Console.WriteLine();
    Console.WriteLine("--------------------");
    Console.WriteLine();
  }

  private static void PrintReference<T>(ConditionalWeakTable<T, object?> table, string title) where T : class
  {
    Console.WriteLine();
    Console.BackgroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"++ {title} ++");
    Console.BackgroundColor = ConsoleColor.White;
    foreach (var kv in table)
    {
      Console.WriteLine(kv.Value);
    }
  }

  private void DebugWindow_GCButton_Click(object sender, RoutedEventArgs e)
  {
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
  }

  private void DebugWindow_NewNoteButton_Click(object sender, RoutedEventArgs e)
  {
    Note note = new() { Created = DateTimeOffset.UtcNow, Id = NoteId.NewId(), NavigationId = NavigationId.NewId() };
    note.Title = $"New note {note.Id.Value}";
    var noteService = App.Instance.Services.GetRequiredService<NoteService>();
    noteService.OpenNoteWindow(note);
  }

  private void DebugWindow_MainWindowButton_Click(object sender, RoutedEventArgs e)
  {
    var windowService = App.Instance.Services.GetRequiredService<WindowService>();
    windowService.MainWindow.Activate();
  }

  private async void DebugWindow_ClearDatabaseButton_Click(object sender, RoutedEventArgs e)
  {
    var factory = App.Instance.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var context = await factory.CreateDbContextAsync();
    await context.Database.EnsureDeletedAsync();
  }

  private async void DebugWindow_CreateDatabaseButton_Click(object sender, RoutedEventArgs e)
  {
    var factory = App.Instance.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var context = await factory.CreateDbContextAsync();
    await context.Database.EnsureCreatedAsync();
  }
}
