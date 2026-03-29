using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Helpers;

using Microsoft.UI.Xaml.Documents;

using MyNotes.AppConstants;
using MyNotes.Helpers;
using MyNotes.Models.Media;
using MyNotes.Models.Navigations;

namespace MyNotes.Models.Notes;

[Debugging.ReferenceTracker]
internal sealed partial class Note : ObservableObject, IComparable<Note>
{
  public Note()
  {
    TrackReference();
  }

  public required NoteId Id { get; init; }

  [ObservableProperty]
  public required partial NavigationId NavigationId { get; set; }

  public required DateTimeOffset Created { get; init; }

  [ObservableProperty]
  public partial DateTimeOffset Modified { get; private set; }
  private void UpdateModified() => Modified = DateTimeOffset.UtcNow;

  [ObservableProperty]
  public partial string Title { get; set; } = string.Empty;

  partial void OnTitleChanged(string oldValue, string newValue) => UpdateModified();

  [ObservableProperty]
  public partial string Body { get; set; } = string.Empty;

  partial void OnBodyChanged(string oldValue, string newValue) => UpdateModified();

  [ObservableProperty]
  public partial string BodyPlainText { get; set; } = string.Empty;

  public List<TextRange> HighlighterRanges { get; } = new();

  [ObservableProperty]
  public partial Color BackgroundColor { get; set; } = AppSettingsDescriptors.NoteBackground.DefaultValue.ToColor();

  partial void OnBackgroundColorChanged(Color oldValue, Color newValue) => UpdateModified();

  [ObservableProperty]
  public partial bool ShowBackgroundImage { get; set; }

  [ObservableProperty]
  public partial string? BackgroundImagePath { get; set; }

  [ObservableProperty]
  public partial double BackgroundImageOpacity { get; set; }

  [ObservableProperty]
  public partial double BackgroundImageBlur { get; set; }

  [ObservableProperty]
  public partial BackdropKind BackdropKind { get; set; } = (BackdropKind)AppSettingsDescriptors.NoteBackdropKind.DefaultValue;

  partial void OnBackdropKindChanged(BackdropKind oldValue, BackdropKind newValue) => UpdateModified();

  [ObservableProperty]
  public partial double BackdropTintOpacity { get; set; }

  [ObservableProperty]
  public partial double BackdropLuminosityOpacity { get; set; }

  [ObservableProperty]
  public partial ImmutableList<ImageDescriptor> Images { get; set; } = [];

  [ObservableProperty]
  public partial bool ShowImagePanel { get; set; }

  [ObservableProperty]
  public partial double ImagePanelHeight { get; set; }

  [ObservableProperty]
  public partial SizeInt32 Size { get; set; } = AppSettingsDescriptors.NoteSize.DefaultValue.SizeInt32;

  [ObservableProperty]
  public partial PointInt32 Position { get; set; } = AppSettingsDescriptors.NotePosition.DefaultValue.PointInt32;

  [ObservableProperty]
  public partial bool IsBookmarked { get; set; }

  [ObservableProperty]
  public partial bool IsDeleted { get; set; }

  [ObservableProperty]
  public partial bool IsWindowOpen { get; set; }

  [ObservableProperty]
  public partial bool IsAlwaysOnTop { get; set; }

  public int CompareTo(Note? other) => other is null ? 1 : Created.CompareTo(other.Created);
}
