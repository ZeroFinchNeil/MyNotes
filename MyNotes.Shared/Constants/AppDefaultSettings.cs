
using System;
using System.Collections.Generic;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

using MyNotes.Shared.Enums.Media;
using MyNotes.Shared.Enums.Navigations;
using MyNotes.Shared.Enums.Notes;
using MyNotes.Shared.Enums.Settings;

using Windows.Foundation;

namespace MyNotes.Shared.Constants;

internal sealed class AppDefaultSettings
{
  // Windows
  public static readonly bool IsMainWindowOpen = true;
  public static readonly Size MainWindowMinimumSize = new(600.0, 600.0);
  public static readonly Size MainWindowSize = new(600.0, 800.0);
  public static readonly Point MainWindowPosition = new(0.0, 0.0);
  public static readonly string MainWindowDisplay = string.Empty;
  public static readonly int WindowBorderMargin = 20;

  public static readonly Point WindowPosition = new(32.0, 32.0);

  // Settings - Appearance
  public static readonly ElementTheme AppTheme = ElementTheme.Default;
  public static readonly string AppLanguage = string.Empty;

  // Settings - General
  public static readonly int InitialPageType = 0;
  public static readonly Guid InitialPageId = AppNavigationGuids.HomeId;
  public static readonly bool ConfirmBeforeDeleting = true;

  // Settings - Note
  public static readonly Size NoteWindowMinimumSize = new(400.0, 300.0);

  public static readonly string NoteBackground = "#fff2e28d";
  public static readonly BackdropKind NoteBackdropKind = BackdropKind.None;
  public static readonly Size NoteSize = new(500.0, 500.0);
  public static readonly Point NotePosition = new(0, 0);

  public static readonly int NoteBodyUpdateFrequency = 2;

  public static readonly bool DeleteEmptyNote = true;

  // Settings - List and Group
  public static readonly bool ShowNoteCount = true;
  public static readonly GroupIconBadge GroupIconBadge = GroupIconBadge.Folder;

  public static readonly bool AllowCustomNoteSortOrder = true;
  public static readonly NoteSortKey NoteSortKey = NoteSortKey.Created;
  public static readonly SortDirection NoteSortDirection = SortDirection.Descending;

  public static readonly bool AllowCustomPreviewLayout = true;
  public static readonly PreviewLayoutType PreviewLayoutType = PreviewLayoutType.Grid;
  public static readonly PreviewTileSize PreviewTileSize = PreviewTileSize.Medium;
  public static readonly PreviewTileRatio PreviewTileRatio = PreviewTileRatio.Square;

  // Note
  public static readonly string NoteTitle = string.Empty;
  public static readonly string NoteBodyRtfText = string.Empty;
  public static readonly string NoteBodyPlainText = string.Empty;
  public static readonly bool IsNoteBookmarked = false;
  public static readonly bool IsNoteDeleted = false;

  public static readonly bool ShowNoteBackgroundImage = false;
  public static readonly Stretch NoteBackgroundImageStretch = Stretch.Uniform;
  public static readonly AlignmentPosition NoteBackgroundImageAlignment = AlignmentPosition.Center;
  public static readonly string? NoteBackgroundImagePath = null;
  public static readonly double NoteBackgroundImageOpacity = 1.0;
  public static readonly int NoteBackgroundImageBlur = 0;
  public static readonly double NoteBackdropTintOpacity = 0.5;
  public static readonly double NoteBackdropLuminosityOpacity = 0.5;
  public static readonly IReadOnlyList<string> NoteBodyImagePaths = [];
  public static readonly bool ShowNoteImagePanel = false;
  public static readonly double NoteImagePanelHeight = 120.0;
  public static readonly bool IsNoteTextEditorReadOnly = false;
  public static readonly bool IsNoteWindowOpen = true;
  public static readonly bool IsNoteWindowAlwaysOnTop = false;

  // Navigation
  public static readonly int GroupNavigationIcon = 3271; // Icon.System_Notebook
  public static readonly int ListNavigationIcon = 3163; // Icon.System_Board
  public static readonly bool IsNavigationDeleted = false;
}