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

  public static readonly string NoteTitle = string.Empty;
  public static readonly string NoteBodyRtfText = string.Empty;
  public static readonly string NoteBodyPlainText = string.Empty;
  public static readonly bool IsNoteBookmarked = false;
  public static readonly bool IsNoteDeleted = false;

  public static readonly bool ShowNoteBackgroundImage = false;
  public static readonly int NoteBackgroundImageStretch = 2;
  public static readonly AlignmentPosition NoteBackgroundImageAlignment = AlignmentPosition.Center;
  public static readonly string? NoteBackgroundImagePath = null;
  public static readonly double NoteBackgroundImageOpacity = 1.0;
  public static readonly int NoteBackgroundImageBlur = 0;
  public static readonly double NoteBackdropTintOpacity = 0.5;
  public static readonly double NoteBackdropLuminosityOpacity = 0.5;
  public static readonly bool ShowNoteImagePanel = true;
  public static readonly double NoteImagePanelHeight = 120.0;
  public static readonly bool IsNoteTextEditorReadOnly = false;
  public static readonly bool IsNoteWindowOpen = true;
  public static readonly bool IsNoteWindowAlwaysOnTop = false;
}