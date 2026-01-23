using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Helpers;

using Microsoft.UI.Xaml.Documents;

using MyNotes.Constants;
using MyNotes.Helpers;
using MyNotes.Models.Navigations;

namespace MyNotes.Models.Notes;

internal sealed class Note : ObservableObject, IComparable<Note>
{
  public required NoteId Id { get; init; }

  public required NavigationId NavigationId
  {
    get => field;
    set => SetProperty(ref field, value);
  }

  public required DateTimeOffset Created { get; init; }

  public DateTimeOffset Modified
  {
    get => field;
    private set => SetProperty(ref field, value);
  }

  public string Title
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
  } = string.Empty;

  public string Body
  {
    get;
    set
    {
      if (field != value)
      {
        SetProperty(ref field, value);
        Modified = DateTimeOffset.UtcNow;
      }
    }
  } = string.Empty;

  public string BodyPlainText
  {
    get;
    set
    {
      if (field != value)
      {
        SetProperty(ref field, value);
      }
    }
  } = string.Empty;

  public List<TextRange> HighlighterRanges { get; } = new();

  public Color Background
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
  } = SettingsDescriptors.NoteBackground.DefaultValue.ToColor();

  public BackdropKind Backdrop
  {
    get;
    set
    {
      if (field != value)
      {
        SetProperty(ref field, value);
        Modified = DateTimeOffset.UtcNow;
      }
    }
  } = (BackdropKind)SettingsDescriptors.NoteBackdrop.DefaultValue;

  public SizeInt32 Size
  {
    get => field;
    set => SetProperty(ref field, value);
  } = SettingsDescriptors.NoteSize.DefaultValue.SizeInt32;

  public PointInt32 Position
  {
    get => field;
    set => SetProperty(ref field, value);
  } = SettingsDescriptors.NotePosition.DefaultValue.PointInt32;

  public bool IsBookmarked
  {
    get;
    set => SetProperty(ref field, value);
  }

  public bool IsDeleted
  {
    get;
    set => SetProperty(ref field, value);
  }

  public int CompareTo(Note? other) => other is null ? 1 : Created.CompareTo(other.Created);
}
