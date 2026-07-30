using MyNotes.Application.Contracts.Media.Models;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Common.Structures;

namespace MyNotes.Application.Notes;

internal static class NoteSettingsDescriptors
{
  public static SettingsDescriptor<string> NoteBackground { get; } = new()
  {
    Key = "NoteBackground",
    DefaultValue = "#fff2e28d"
  };

  public static SettingsDescriptor<BackdropKind> NoteBackdropKind { get; } = new()
  {
    Key = "NoteBackdropKind",
    DefaultValue = BackdropKind.None
  };

  public static string NoteTitle { get; } = string.Empty;
  public static string NoteBodyRtfText { get; } = string.Empty;
  public static string NoteBodyPlainText { get; } = string.Empty;
  public static bool IsNoteBookmarked { get; } = false;
  public static bool IsNoteDeleted { get; } = false;

  public static bool ShowNoteBackgroundImage { get; } = false;
  public static int NoteBackgroundImageStretch { get; } = 2;
  public static AlignmentPosition NoteBackgroundImageAlignment { get; } = AlignmentPosition.Center;
  public static string? NoteBackgroundImagePath { get; } = null;
  public static double NoteBackgroundImageOpacity { get; } = 1.0;
  public static int NoteBackgroundImageBlur { get; } = 0;
  public static double NoteBackdropTintOpacity { get; } = 0.5;
  public static double NoteBackdropLuminosityOpacity { get; } = 0.5;
  public static IReadOnlyList<string> NoteBodyImagePaths { get; } = [];
  public static bool ShowNoteImagePanel { get; } = false;
  public static double NoteImagePanelHeight { get; } = 120.0;
  public static bool IsNoteTextEditorReadOnly { get; } = false;
  public static bool IsNoteWindowOpen { get; } = true;
  public static bool IsNoteWindowAlwaysOnTop { get; } = false;
}