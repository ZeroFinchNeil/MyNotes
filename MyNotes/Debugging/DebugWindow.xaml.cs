using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using MyNotes.Services.App;
using MyNotes.Services.Database;

namespace MyNotes.Debugging;

internal sealed partial class DebugWindow : Window
{
  public DebugWindow()
  {
    InitializeComponent();
    this.AppWindow.MoveAndResize(new(0, 0, 700, 300));
  }

  private async void DebugWindow_SeparatorButton_Click(object sender, RoutedEventArgs e)
  {
    PrintSeparator();
  }

  private void DebugWindow_GCButton_Click(object sender, RoutedEventArgs e)
  {
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
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
    Console.WriteLine();
    Console.WriteLine("-------------------------------------------------------------------------------");
    Console.WriteLine();
  }

  private void DebugWindow_ShowReferencesButton_Click(object sender, RoutedEventArgs e)
  {
    PrintSeparator();
    var table = ReferenceTracker.GetAliveReferences().OrderBy(pair => pair.Key.Name);
    var paddingColor = _consoleColors[_colorCount++ % _consoleColors.Count];
    foreach (var pair in table)
    {
      Console.BackgroundColor = paddingColor;
      Console.Write(" ");
      Console.BackgroundColor = ConsoleColor.White;
      Console.WriteLine(" {0, 30} : {1}", pair.Key.Name, string.Join(", ", pair.Value.Where(obj => obj is not null).Select(obj => obj!.GetHashCode())));
      //Console.BackgroundColor = paddingColor;
      //Console.WriteLine(" ");
      //Console.BackgroundColor = ConsoleColor.White;
    }
    PrintSeparator();
  }

  private async void DebugWindow_MainWindowButton_Click(object sender, RoutedEventArgs e)
  {
    var windowService = App.Services.GetRequiredService<WindowService>();
    (await windowService.GetOrCreateMainWindow()).Activate();
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

  private void DebugWindow_TrackFocusedElementToggleButton_Click(object sender, RoutedEventArgs e)
  {
    FocusManager.GotFocus -= FocusManager_GotFocus;
    DebugWindow_FocusedElementTextBlock.Text = string.Empty;

    if (DebugWindow_TrackFocusedElementToggleButton.IsChecked is bool boolValue && boolValue)
    {
      FocusManager.GotFocus += FocusManager_GotFocus;
    }
  }

  private async void FocusManager_GotFocus(object? sender, FocusManagerGotFocusEventArgs e)
  {
    StringBuilder sb = new();
    var windowService = App.Services.GetRequiredService<WindowService>();
    if (windowService.TryGetCurrentMainWindow(out var mainWindow)
      && FocusTracker.GetFocusedElement(mainWindow.Content.XamlRoot) is FrameworkElement mainWindowElement)
    {
      sb.AppendLine($"Main: [{mainWindowElement.GetType().Name}] {mainWindowElement.Name}");
    }

    if (windowService.TryGetCurrentImageViewerWindow(out var imageWindow)
      && FocusTracker.GetFocusedElement(imageWindow.Content.XamlRoot) is FrameworkElement imageWindowElement)
    {
      sb.AppendLine($"ImageViewer: [{imageWindowElement.GetType().Name}] {imageWindowElement.Name}");
    }

    foreach (var wr in windowService.NoteWindowTable.Values)
    {
      if (wr.TryGetTarget(out var noteWindow))
      {
        if (FocusTracker.GetFocusedElement(noteWindow.Content.XamlRoot) is FrameworkElement noteWindowElement)
        {
          sb.AppendLine($"Note: [{noteWindowElement.GetType().Name}] {noteWindowElement.Name}");
        }
      }
    }

    DebugWindow_FocusedElementTextBlock.Text = sb.ToString();

    DebugWindow_FocusedElementBorder.BorderBrush = new SolidColorBrush(Colors.DarkGray);
    await Task.Delay(500);
    DebugWindow_FocusedElementBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);
  }
}
