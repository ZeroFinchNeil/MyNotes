using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.Database;
using MyNotes.Services.Window;
using MyNotes.Views.Windows;

namespace MyNotes.Debugging;

public sealed partial class DebugWindow : Window
{
  public DebugWindow()
  {
    InitializeComponent();
    this.AppWindow.Resize(new(500, 500));
  }

  private async void DebugWindow_SeparatorButton_Click(object sender, RoutedEventArgs e)
  {
    Console.WriteLine();
    Console.WriteLine("--------------------");
    Console.WriteLine();
  }

  private async void DebugWindow_DebugButton_Click(object sender, RoutedEventArgs e)
  {
    var factory = App.Instance.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var context = await factory.CreateDbContextAsync();
    foreach (var entity in await context.NavigationEntities.ToListAsync())
    {
      Console.WriteLine(entity.ToString());
      Console.WriteLine();
    }
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

  private void DebugWindow_GCButton_Click(object sender, RoutedEventArgs e)
  {
    GC.Collect();
    WindowService windowService = App.Instance.Services.GetRequiredService<WindowService>();
    Console.WriteLine();
    Console.WriteLine("--------------------");
    Console.WriteLine("Main Window");
    if (windowService.MainWindow is not null && windowService.MainWindow.TryGetTarget(out var mainWindow))
      Console.WriteLine("{0}: {1}", mainWindow, "True");

    Console.WriteLine();
    Console.WriteLine("Note Windows");
    foreach (var kv in windowService.NoteWindows)
    {
      bool res = kv.Value.TryGetTarget(out var noteWindow);
      Console.WriteLine("{0}: {1}", kv.Key.Id.Value, res);
    }

    Console.WriteLine();
    Console.WriteLine("Blank Windows");
    foreach (var kv in windowService.BlankWindows)
    {
      bool res = kv.Value.TryGetTarget(out var blankWindow);
      Console.WriteLine("{0}: {1}", kv.Key, res);
    }

    Console.WriteLine("--------------------");
    Console.WriteLine();
  }

  private void DebugWindow_NewNoteButton_Click(object sender, RoutedEventArgs e)
  {
    Note note = new() { Created = DateTimeOffset.UtcNow, Id = NoteId.NewId(), NavigationId = NavigationId.NewId() };
    new NoteWindow(note).Activate();

  }

  private void DebugWindow_BlankWindowButton_Click(object sender, RoutedEventArgs e)
  {
    var window = new BlankWindow();
    window.Activate();
    WindowService windowService = App.Instance.Services.GetRequiredService<WindowService>();
    windowService.BlankWindows.Add(Guid.NewGuid(), new(window));
  }
}
