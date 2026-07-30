namespace MyNotes.Application.Contracts.Settings;

internal sealed class AppDefaultSettings
{
  // Navigation
  public static int GroupNavigationIcon { get; } = 3271; // Icon.System_Notebook
  public static int ListNavigationIcon { get; } = 3163; // Icon.System_Board
  public static bool IsNavigationDeleted { get; } = false;
}