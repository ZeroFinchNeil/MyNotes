using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.Database;
using MyNotes.Services.Notes;
using MyNotes.Services.Windows;
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
    Console.WriteLine();
    PrintSeparator();
    PrintReference(ReferenceTracker.WindowReference, "Windows");
    PrintReference(ReferenceTracker.PageReference, "Pages");
    PrintReference(ReferenceTracker.ViewModelReference, "ViewModels");

    PrintReference(ReferenceTracker.NavigationReference, "Navigations");
    PrintReference(ReferenceTracker.NoteReference, "Notes");

    PrintReference(ReferenceTracker.ElementReference, "Elements");
    PrintPadding();
    PrintSeparator();
    Console.WriteLine();

    _colorCount++;
  }

  private static readonly List<ConsoleColor> _consoleColors = new()
  {
    ConsoleColor.Red,
    ConsoleColor.Green,
    ConsoleColor.Blue,
  };
  private static int _colorCount = 0;

  private static void PrintSeparator()
  {
    Console.BackgroundColor = _consoleColors[_colorCount % _consoleColors.Count];
    Console.WriteLine($"{"",90}");
    Console.BackgroundColor = ConsoleColor.White;
  }

  private static void PrintPadding(string? text = null, ConsoleColor color = ConsoleColor.White)
  {
    text ??= string.Empty;
    var paddingColor = _consoleColors[_colorCount % _consoleColors.Count];
    Console.BackgroundColor = paddingColor;
    Console.Write("  ");
    Console.BackgroundColor = ConsoleColor.White;
    Console.Write(" ");
    Console.BackgroundColor = color;
    Console.Write($"{text,-84}");
    Console.BackgroundColor = ConsoleColor.White;
    Console.Write(" ");
    Console.BackgroundColor = paddingColor;
    Console.WriteLine("  ");
    Console.BackgroundColor = ConsoleColor.White;
  }

  private static void PrintReference<T>(ConditionalWeakTable<T, object?> table, string title) where T : class
  {
    PrintPadding();
    PrintPadding($"++ {title} ++", ConsoleColor.Yellow);

    foreach (var kv in table)
    {
      PrintPadding(kv.Value?.ToString());
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
    var noteService = App.Services.GetRequiredService<NoteService>();
    noteService.OpenNoteWindow(note);
  }

  private void DebugWindow_MainWindowButton_Click(object sender, RoutedEventArgs e)
  {
    var windowService = App.Services.GetRequiredService<WindowService>();
    windowService.MainWindow?.Activate();
  }

  private async void DebugWindow_ClearDatabaseButton_Click(object sender, RoutedEventArgs e)
  {
    var factory = App.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var context = await factory.CreateDbContextAsync();
    await context.Database.EnsureDeletedAsync();
  }

  private async void DebugWindow_CreateDatabaseButton_Click(object sender, RoutedEventArgs e)
  {
    var factory = App.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var context = await factory.CreateDbContextAsync();
    await context.Database.EnsureCreatedAsync();
  }
}
