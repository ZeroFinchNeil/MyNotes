using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Helpers;

using Microsoft.UI.Xaml.Documents;

using MyNotes.Common.Helpers;
using MyNotes.Domain.ValueObjects;
using MyNotes.Models.Media;
using MyNotes.Shared.Constants;
using MyNotes.Shared.Enums.Media;
using MyNotes.Shared.Enums.Notes;

namespace MyNotes.Models.Notes;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class NoteModel : ObservableObject, IComparable<NoteModel>
{
  public NoteModel()
  {
    TrackReference();
  }

  // Domain Properties
  public required NoteId Id { get; init; }

  [ObservableProperty]
  public required partial NavigationId NavigationId { get; set; }

  public required DateTimeOffset Created { get; init; }

  [ObservableProperty]
  public partial DateTimeOffset Modified { get; set; }

  [ObservableProperty]
  public partial string Title { get; set; } = string.Empty;

  [ObservableProperty]
  public partial string Body { get; set; } = string.Empty;

  [ObservableProperty]
  public partial ImmutableList<ImageDescriptor> BodyImagePaths { get; set; } = [];

  [ObservableProperty]
  public partial Color BackgroundColor { get; set; } = AppDefaultSettings.NoteBackground.ToColor();

  [ObservableProperty]
  public partial string? BackgroundImagePath { get; set; }

  [ObservableProperty]
  public partial bool IsBookmarked { get; set; }

  [ObservableProperty]
  public partial bool IsDeleted { get; set; }

  // Presentation Properties
  [ObservableProperty]
  public partial bool ShowBackgroundImage { get; set; }

  public Stretch BackgroundImageStretch
  {
    get;
    set
    {
      if (Enum.IsDefined(value))
      {
        SetProperty(ref field, value);
      }
    }
  }

  public AlignmentPosition BackgroundImageAlignment
  {
    get;
    set
    {
      if (Enum.IsDefined(value))
      {
        SetProperty(ref field, value);
      }
    }
  }

  [ObservableProperty]
  public partial double BackgroundImageOpacity { get; set; }

  [ObservableProperty]
  public partial double BackgroundImageBlur { get; set; }

  public BackdropKind BackdropKind
  {
    get;
    set
    {
      if (Enum.IsDefined(value))
      {
        SetProperty(ref field, value);
      }
    }
  } = AppDefaultSettings.NoteBackdropKind;

  [ObservableProperty]
  public partial double BackdropTintOpacity { get; set; }

  [ObservableProperty]
  public partial double BackdropLuminosityOpacity { get; set; }

  [ObservableProperty]
  public partial bool ShowImagePanel { get; set; }

  [ObservableProperty]
  public partial double ImagePanelHeight { get; set; }

  [ObservableProperty]
  public partial SizeInt32 Size { get; set; } = AppDefaultSettings.NoteSize.SizeInt32;

  [ObservableProperty]
  public partial PointInt32 Position { get; set; } = AppDefaultSettings.NotePosition.PointInt32;

  [ObservableProperty]
  public partial bool IsTextEditorReadOnly { get; set; }

  [ObservableProperty]
  public partial bool IsWindowOpen { get; set; }

  [ObservableProperty]
  public partial bool IsAlwaysOnTop { get; set; }

  // View-only Properties
  public List<TextRange> HighlighterRanges { get; } = new();

  [ObservableProperty]
  public partial string Preview { get; set; } = string.Empty;

  public int CompareTo(NoteModel? other) => other is null ? 1 : Created.CompareTo(other.Created);
}
