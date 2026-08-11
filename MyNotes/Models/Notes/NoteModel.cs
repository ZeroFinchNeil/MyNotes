using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Documents;

using MyNotes.Application.Contracts.Media.Models;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Debugging.Attributes;
using MyNotes.Domain.Navigations;
using MyNotes.Domain.Notes;

namespace MyNotes.Models.Notes;

[ReferenceTracker]
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
  public required partial DateTimeOffset Modified { get; set; }

  [ObservableProperty]
  public required partial string Title { get; set; }

  [ObservableProperty]
  public required partial string Body { get; set; }

  [ObservableProperty]
  public required partial Color BackgroundColor { get; set; }

  [ObservableProperty]
  public required partial string? BackgroundImagePath { get; set; }

  [ObservableProperty]
  public required partial bool IsBookmarked { get; set; }

  [ObservableProperty]
  public required partial bool IsDeleted { get; set; }

  // Presentation Properties
  [ObservableProperty]
  public required partial bool ShowBackgroundImage { get; set; }

  public required Stretch BackgroundImageStretch
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

  public required AlignmentPosition BackgroundImageAlignment
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
  public required partial double BackgroundImageOpacity { get; set; }

  [ObservableProperty]
  public required partial double BackgroundImageBlur { get; set; }

  public required BackdropKind BackdropKind
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
  public required partial double BackdropTintOpacity { get; set; }

  [ObservableProperty]
  public required partial double BackdropLuminosityOpacity { get; set; }

  [ObservableProperty]
  public required partial bool ShowImagePanel { get; set; }

  [ObservableProperty]
  public required partial double ImagePanelHeight { get; set; }

  [ObservableProperty]
  public required partial SizeInt32 Size { get; set; }

  [ObservableProperty]
  public required partial PointInt32 Position { get; set; }

  [ObservableProperty]
  public required partial bool IsTextEditorReadOnly { get; set; }

  [ObservableProperty]
  public required partial bool IsWindowOpen { get; set; }

  [ObservableProperty]
  public required partial bool IsAlwaysOnTop { get; set; }

  // View-only Properties
  public List<TextRange> HighlighterRanges { get; } = new();

  [ObservableProperty]
  public partial string Preview { get; set; } = string.Empty;

  public int CompareTo(NoteModel? other) => other is null ? 1 : Created.CompareTo(other.Created);
}
