using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Helpers;

using Microsoft.UI.Xaml.Documents;

using MyNotes.AppConstants;
using MyNotes.Debugging;
using MyNotes.Helpers;
using MyNotes.Models.Navigations;

namespace MyNotes.Models.Notes;

internal sealed partial class Note : ObservableObject, IComparable<Note>
{
  public Note()
  {
#if DEBUG
    if (Debugger.IsAttached)
    {
      ReferenceTracker.NoteReference.Add(this, $"{GetType().Name}: {GetHashCode()}");
    }
#endif
  }

  public required NoteId Id { get; init; }

  [ObservableProperty]
  public required partial NavigationId NavigationId { get; set; }

  public required DateTimeOffset Created { get; init; }

  [ObservableProperty]
  public partial DateTimeOffset Modified { get; private set; }

  public string Title
  {
    get => field;
    set
    {
      if (SetProperty(ref field, value))
      {
        Modified = DateTimeOffset.UtcNow;
      }
    }
  } = string.Empty;

  public string Body
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        Modified = DateTimeOffset.UtcNow;
      }
    }
  } = string.Empty;

  [ObservableProperty]
  public partial string BodyPlainText { get; set; } = string.Empty;

  public List<TextRange> HighlighterRanges { get; } = new();

  public Color BackgroundColor
  {
    get => field;
    set
    {
      if (field != value)
      {
        SetProperty(ref field, value);
        Modified = DateTimeOffset.UtcNow;
      }
    }
  } = AppSettingsDescriptors.NoteBackground.DefaultValue.ToColor();

  [ObservableProperty]
  public partial bool ShowBackgroundImage { get; set; }

  [ObservableProperty]
  public partial string? BackgroundImagePath { get; set; }

  [ObservableProperty]
  public partial double BackgroundImageOpacity { get; set; }

  [ObservableProperty]
  public partial double BackgroundImageBlur { get; set; }

  public BackdropKind BackdropKind
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        Modified = DateTimeOffset.UtcNow;
      }
    }
  } = (BackdropKind)AppSettingsDescriptors.NoteBackdropKind.DefaultValue;

  [ObservableProperty]
  public partial double BackdropTintOpacity { get; set; }

  [ObservableProperty]
  public partial double BackdropLuminosityOpacity { get; set; }

  [ObservableProperty]
  public partial ImmutableList<string> Images { get; set; } = [];

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
