using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Windows.Storage.Pickers;

using MyNotes.Application.Dtos.Notes.Modification;
using MyNotes.Application.Services.Notes;
using MyNotes.Common.Collections;
using MyNotes.Common.Commands;
using MyNotes.Common.Helpers;
using MyNotes.Models.Notes;
using MyNotes.Services.Windows;
using MyNotes.Shared.Constants;
using MyNotes.Shared.Enums.Media;
using MyNotes.Shared.Enums.Notes;
using MyNotes.Shell.Contracts.Converters;
using MyNotes.Templates.Media;

namespace MyNotes.ViewModels.Notes;

internal sealed partial class NoteEditorViewModel : ViewModelBase, IAsyncDisposable
{
  private readonly NoteService NoteService;
  private readonly NoteWindowService NoteWindowService;
  private readonly IRtfTextConverter RtfTextConverter;
  private readonly NoteModel Note;
  private readonly RichEditTextDocument Document;

  #region Object Lifetime Management
  public NoteEditorViewModel(NoteService noteService, NoteWindowService noteWindowService, IRtfTextConverter rtfTextConverter, NoteModel note, RichEditTextDocument document)
  {
    NoteService = noteService;
    NoteWindowService = noteWindowService;
    RtfTextConverter = rtfTextConverter;

    Note = note;
    Document = document;
    _notePropertyDebounceTCS.TrySetResult();
    Note.PropertyChanged += Note_PropertyChanged;
    _editorThrottleTimer.Tick += EditorDebounceTimer_Tick;
    _notePropertyDebounceTimer.Tick += NotePropertyDebounceTimer_Tick;
    _selectedPaletteBackgroundColor = PaletteBackgroundColors.FirstOrDefault(b => b.Color == Note.BackgroundColor);
    _backgroundImageAlignmentX = Note.BackgroundImageAlignment switch
    {
      AlignmentPosition.TopLeft or AlignmentPosition.CenterLeft or AlignmentPosition.BottomLeft => AlignmentX.Left,
      AlignmentPosition.TopCenter or AlignmentPosition.Center or AlignmentPosition.BottomCenter => AlignmentX.Center,
      AlignmentPosition.TopRight or AlignmentPosition.CenterRight or AlignmentPosition.BottomRight => AlignmentX.Right,
      _ => throw new InvalidOperationException()
    };
    _backgroundImageAlignmentY = Note.BackgroundImageAlignment switch
    {
      AlignmentPosition.TopLeft or AlignmentPosition.TopCenter or AlignmentPosition.TopRight => AlignmentY.Top,
      AlignmentPosition.CenterLeft or AlignmentPosition.Center or AlignmentPosition.CenterRight => AlignmentY.Center,
      AlignmentPosition.BottomLeft or AlignmentPosition.BottomCenter or AlignmentPosition.BottomRight => AlignmentY.Bottom,
      _ => throw new InvalidOperationException()
    };
    SetCommands();
  }

  protected override void Dispose(bool disposing)
  {
    if (Disposed)
    {
      return;
    }

    if (disposing)
    {

    }

    base.Dispose(disposing);
  }

  private async ValueTask DisposeAsync(bool disposing)
  {
    if (Disposed)
    {
      return;
    }

    if (disposing)
    {
      Note.PropertyChanged -= Note_PropertyChanged;

      await NotePropertyDebounceTask;

      _editorThrottleTimer.Tick -= EditorDebounceTimer_Tick;
      _notePropertyDebounceTimer.Tick -= NotePropertyDebounceTimer_Tick;
    }
  }

  public async ValueTask DisposeAsync()
  {
    await DisposeAsync(disposing: true).ConfigureAwait(false);
    Dispose(disposing: false);
  }
  #endregion

  private readonly DispatcherTimer _notePropertyDebounceTimer = new() { Interval = TimeSpan.FromMilliseconds(500) };

  private TaskCompletionSource _notePropertyDebounceTCS = new();
  private Task NotePropertyDebounceTask => _notePropertyDebounceTCS.Task;

  private async void NotePropertyDebounceTimer_Tick(object? sender, object e)
  {
    _notePropertyDebounceTimer.Stop();
    await UpdateNoteAsync();
    _notePropertyDebounceTCS.TrySetResult();
  }

  [Flags]
  private enum NoteDebouncingProperties
  {
    None = 0,
    Body = 1 << 0,
    BackgroundColor = 1 << 1,
  }

  private enum NoteViewStateDebouncingProperties
  {
    None = 0,
    BackgroundImageOpacity = 1 << 0,
    BackgroundImageBlur = 1 << 1,
    BackdropTintOpacity = 1 << 2,
    BackdropLuminosityOpacity = 1 << 3,
    ImagePanelHeight = 1 << 4,
    Size = 1 << 5,
    Position = 1 << 6
  }

  private NoteDebouncingProperties _noteDebouncingProperties = NoteDebouncingProperties.None;
  private NoteViewStateDebouncingProperties _noteViewStateDebouncingProperties = NoteViewStateDebouncingProperties.None;

  private async void Note_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    _notePropertyDebounceTimer.Stop();

    // 뷰에 반영(TwoWay 바인딩 시) 
    switch (e.PropertyName)
    {
      case nameof(Note.Body):  // * Debouncing
        _noteDebouncingProperties |= NoteDebouncingProperties.Body;
        break;
      case nameof(Note.BackgroundColor):  // * Debouncing
        SelectedPaletteBackgroundColor = PaletteBackgroundColors.FirstOrDefault(b => b.Color == Note.BackgroundColor);
        ChangeSystemBackdropExtended();
        _noteDebouncingProperties |= NoteDebouncingProperties.BackgroundColor;
        break;
      case nameof(Note.BackgroundImagePath):
        await NoteService.Modification.UpdateNoteAsync(new() { Id = Note.Id, BackgroundImagePath = Note.BackgroundImagePath });
        break;
      case nameof(Note.BackgroundImageStretch):
        await NoteService.Modification.UpdateNoteViewStateAsync(new() { Id = Note.Id, BackgroundImageStretch = (int)Note.BackgroundImageStretch });
        break;
      case nameof(Note.BackgroundImageAlignment):
        await NoteService.Modification.UpdateNoteViewStateAsync(new() { Id = Note.Id, BackgroundImageAlignment = (int)Note.BackgroundImageAlignment });
        break;
      case nameof(Note.BackgroundImageOpacity):  // * Debouncing
        _noteViewStateDebouncingProperties |= NoteViewStateDebouncingProperties.BackgroundImageOpacity;
        break;
      case nameof(Note.BackgroundImageBlur): // * Debouncing
        _noteViewStateDebouncingProperties |= NoteViewStateDebouncingProperties.BackgroundImageBlur;
        break;
      case nameof(Note.BackdropKind):
        ChangeSystemBackdrop();
        ChangeSystemBackdropExtended();
        if (Note.BackdropKind is not BackdropKind.None)
        {
          Note.ShowBackgroundImage = false;
        }
        await NoteService.Modification.UpdateNoteViewStateAsync(new() { Id = Note.Id, BackdropKind = (int)Note.BackdropKind });
        break;
      case nameof(Note.BackdropTintOpacity): // * Debouncing
        ChangeSystemBackdropExtended();
        _noteViewStateDebouncingProperties |= NoteViewStateDebouncingProperties.BackdropTintOpacity;
        break;
      case nameof(Note.BackdropLuminosityOpacity): // * Debouncing
        ChangeSystemBackdropExtended();
        _noteViewStateDebouncingProperties |= NoteViewStateDebouncingProperties.BackdropLuminosityOpacity;
        break;
      case nameof(Note.BodyImagePaths):
        await NoteService.Modification.UpdateNoteAsync(new() { Id = Note.Id, BodyImagePaths = new([.. Note.BodyImagePaths.Select(d => d.FilePath)]) });
        break;
      case nameof(Note.ShowImagePanel):
        await NoteService.Modification.UpdateNoteViewStateAsync(new() { Id = Note.Id, ShowImagePanel = Note.ShowImagePanel });
        break;
      case nameof(Note.ImagePanelHeight): // * Debouncing
        _noteViewStateDebouncingProperties |= NoteViewStateDebouncingProperties.ImagePanelHeight;
        break;
      case nameof(Note.Size): // * Debouncing
        _noteViewStateDebouncingProperties |= NoteViewStateDebouncingProperties.Size;
        break;
      case nameof(Note.Position): // * Debouncing
        _noteViewStateDebouncingProperties |= NoteViewStateDebouncingProperties.Position;
        break;
      case nameof(Note.IsWindowOpen):
        await NoteService.Modification.UpdateNoteViewStateAsync(new() { Id = Note.Id, IsWindowOpen = Note.IsWindowOpen });
        break;
      case nameof(Note.IsTextEditorReadOnly):
        await NoteService.Modification.UpdateNoteViewStateAsync(new() { Id = Note.Id, IsTextEditorReadOnly = Note.IsTextEditorReadOnly });
        break;
      case nameof(Note.IsAlwaysOnTop):
        NoteWindowService.TryExecuteOnWindow(Note.Id, noteWindow => (noteWindow.AppWindow.Presenter as OverlappedPresenter)?.IsAlwaysOnTop = Note.IsAlwaysOnTop);
        await NoteService.Modification.UpdateNoteViewStateAsync(new() { Id = Note.Id, IsAlwaysOnTop = Note.IsAlwaysOnTop });
        break;
    }

    _notePropertyDebounceTimer.Start();
    _notePropertyDebounceTCS = new();
  }

  private async Task UpdateNoteAsync()
  {
    if (_noteDebouncingProperties is not NoteDebouncingProperties.None)
    {
      UpdateNoteAppRequestDto noteRequestDto = new() { Id = Note.Id };
      if (_noteDebouncingProperties.HasFlag(NoteDebouncingProperties.Body))
      {
        noteRequestDto = noteRequestDto with { Body = Note.Body };
      }
      if (_noteDebouncingProperties.HasFlag(NoteDebouncingProperties.BackgroundColor))
      {
        noteRequestDto = noteRequestDto with { BackgroundColor = Note.BackgroundColor.ToString() };
      }

      var noteResponseDto = await NoteService.Modification.UpdateNoteAsync(noteRequestDto);
      if (noteResponseDto.Modified.TryGet(out var modified))
      {
        Note.Modified = modified;
      }

      _noteDebouncingProperties = NoteDebouncingProperties.None;
    }

    if (_noteViewStateDebouncingProperties is not NoteViewStateDebouncingProperties.None)
    {
      UpdateNoteViewStateAppRequestDto viewStateRequestDto = new() { Id = Note.Id };

      if (_noteViewStateDebouncingProperties.HasFlag(NoteViewStateDebouncingProperties.BackgroundImageOpacity))
      {
        viewStateRequestDto = viewStateRequestDto with
        {
          BackgroundImageOpacity = Math.Round(Note.BackgroundImageOpacity, 2, MidpointRounding.AwayFromZero)
        };
      }
      if (_noteViewStateDebouncingProperties.HasFlag(NoteViewStateDebouncingProperties.BackgroundImageBlur))
      {
        viewStateRequestDto = viewStateRequestDto with
        {
          BackgroundImageBlur = Math.Round(Note.BackgroundImageBlur, 2, MidpointRounding.AwayFromZero)
        };
      }
      if (_noteViewStateDebouncingProperties.HasFlag(NoteViewStateDebouncingProperties.BackdropTintOpacity))
      {
        viewStateRequestDto = viewStateRequestDto with
        {
          BackdropTintOpacity = Math.Round(Note.BackdropTintOpacity, 2, MidpointRounding.AwayFromZero)
        };
      }
      if (_noteViewStateDebouncingProperties.HasFlag(NoteViewStateDebouncingProperties.BackdropLuminosityOpacity))
      {
        viewStateRequestDto = viewStateRequestDto with
        {
          BackdropLuminosityOpacity = Math.Round(Note.BackdropLuminosityOpacity, 2, MidpointRounding.AwayFromZero)
        };
      }
      if (_noteViewStateDebouncingProperties.HasFlag(NoteViewStateDebouncingProperties.ImagePanelHeight))
      {
        viewStateRequestDto = viewStateRequestDto with
        {
          ImagePanelHeight = Note.ImagePanelHeight
        };
      }
      if (_noteViewStateDebouncingProperties.HasFlag(NoteViewStateDebouncingProperties.Size))
      {
        viewStateRequestDto = viewStateRequestDto with
        {
          Width = Note.Size.Width,
          Height = Note.Size.Height
        };
      }
      if (_noteViewStateDebouncingProperties.HasFlag(NoteViewStateDebouncingProperties.Position))
      {
        viewStateRequestDto = viewStateRequestDto with
        {
          PositionX = Note.Position.X,
          PositionY = Note.Position.Y
        };
      }

      await NoteService.Modification.UpdateNoteViewStateAsync(viewStateRequestDto);

      _noteViewStateDebouncingProperties = NoteViewStateDebouncingProperties.None;
    }
  }

  #region Body
  public async Task UpdateNoteBodyAsync()
  {
    UpdateNoteAppRequestDto requestDto = new()
    {
      Id = Note.Id,
      Body = new(Note.Body)
    };
    var responseDto = await NoteService.Modification.UpdateNoteAsync(requestDto);
  }
  #endregion

  #region Background
  public IReadOnlyList<SolidColorBrush> PaletteBackgroundColors => AppColors.DefaultPaletteColorBrushes;

  private SolidColorBrush? _selectedPaletteBackgroundColor;
  public SolidColorBrush? SelectedPaletteBackgroundColor
  {
    get => _selectedPaletteBackgroundColor;
    set
    {
      if (SetProperty(ref _selectedPaletteBackgroundColor, value) && value is not null)
      {
        Note.BackgroundColor = value.Color;
      }
    }
  }

  private AlignmentX _backgroundImageAlignmentX;
  public AlignmentX BackgroundImageAlignmentX
  {
    get => _backgroundImageAlignmentX;
    set
    {
      if (Enum.IsDefined(value))
      {
        SetProperty(ref _backgroundImageAlignmentX, value);
        SetBackgroundImageAlignment(value, BackgroundImageAlignmentY);
      }
    }
  }

  private AlignmentY _backgroundImageAlignmentY;
  public AlignmentY BackgroundImageAlignmentY
  {
    get => _backgroundImageAlignmentY;
    set
    {
      if (Enum.IsDefined(value))
      {
        SetProperty(ref _backgroundImageAlignmentY, value);
        SetBackgroundImageAlignment(BackgroundImageAlignmentX, value);
      }
    }
  }

  private void SetBackgroundImageAlignment(AlignmentX x, AlignmentY y)
  {
    Note.BackgroundImageAlignment = (x, y) switch
    {
      (AlignmentX.Left, AlignmentY.Top) => AlignmentPosition.TopLeft,
      (AlignmentX.Left, AlignmentY.Center) => AlignmentPosition.CenterLeft,
      (AlignmentX.Left, AlignmentY.Bottom) => AlignmentPosition.BottomLeft,
      (AlignmentX.Center, AlignmentY.Top) => AlignmentPosition.TopCenter,
      (AlignmentX.Center, AlignmentY.Center) => AlignmentPosition.Center,
      (AlignmentX.Center, AlignmentY.Bottom) => AlignmentPosition.BottomCenter,
      (AlignmentX.Right, AlignmentY.Top) => AlignmentPosition.TopRight,
      (AlignmentX.Right, AlignmentY.Center) => AlignmentPosition.CenterRight,
      (AlignmentX.Right, AlignmentY.Bottom) => AlignmentPosition.BottomRight,
      _ => throw new InvalidOperationException()
    };
  }
  #endregion

  #region Backdrop

  public void ChangeSystemBackdrop()
  {
    NoteWindowService.TryExecuteOnWindow(Note.Id, (noteWindow) =>
    {
      noteWindow.SystemBackdrop = Note.BackdropKind switch
      {
        BackdropKind.None => null,
        BackdropKind.Acrylic => new ExtendedAcrylicBackdrop(),
        BackdropKind.Mica => new ExtendedMicaBackdrop(),
        _ => throw new InvalidOperationException()
      };
    });
  }

  public void ChangeSystemBackdropExtended()
  {
    NoteWindowService.TryExecuteOnWindow(Note.Id, (noteWindow) =>
    {
      if (noteWindow.SystemBackdrop is ExtendedSystemBackdrop backdrop)
      {
        backdrop.TintColor = Note.BackgroundColor;
        backdrop.TintOpacity = Note.BackdropTintOpacity;
        backdrop.LuminosityOpacity = Note.BackdropLuminosityOpacity;
        backdrop.FallbackColor = Common.Helpers.ColorHelper.GetFallbackColor(Note.BackgroundColor, Note.BackdropTintOpacity);
      }
    });
  }
  #endregion

  private bool _isUpdatingSelectionFormatStates = false;
  public void UpdateSelectionFormatStates()
  {
    _isUpdatingSelectionFormatStates = true;
    var characterFormat = Document.Selection.CharacterFormat;
    var paragraphFormat = Document.Selection.ParagraphFormat;

    IsSelectionBold = characterFormat.Bold is FormatEffect.On;
    IsSelectionItalic = characterFormat.Italic is FormatEffect.On;
    IsSelectionUnderlined = characterFormat.Underline is UnderlineType.Single;
    IsSelectionStrikethrough = characterFormat.Strikethrough is FormatEffect.On;
    SelectionFontSizeText = characterFormat.Size > 0 ? characterFormat.Size.ToString() : string.Empty;
    SelectionFontColor = characterFormat.ForegroundColor;
    SelectionHighlightColor = characterFormat.BackgroundColor;
    IsSelectionParagraphList = paragraphFormat.ListType is not (MarkerType.Undefined or MarkerType.None);
    SelectionMarkerType = paragraphFormat.ListType;
    SelectionMarkerStyle = paragraphFormat.ListStyle;
    IsSelectionMarkerStyleEnabled = SelectionMarkerType is not (MarkerType.None or MarkerType.Undefined or MarkerType.Bullet);
    _isUpdatingSelectionFormatStates = false;
  }

  #region Bold, Italic, Underline, Strikethrough
  public bool IsSelectionBold
  {
    get;
    set
    {
      SetProperty(ref field, value);
      if (!_isUpdatingSelectionFormatStates)
      {
        Document.Selection.CharacterFormat.Bold = FormatEffect.Toggle;
      }
    }
  }

  public bool IsSelectionItalic
  {
    get;
    set
    {
      SetProperty(ref field, value);
      if (!_isUpdatingSelectionFormatStates)
      {
        Document.Selection.CharacterFormat.Italic = FormatEffect.Toggle;
      }
    }
  }

  public bool IsSelectionUnderlined
  {
    get;
    set
    {
      SetProperty(ref field, value);
      if (!_isUpdatingSelectionFormatStates)
      {
        Document.Selection.CharacterFormat.Underline = (Document.Selection.CharacterFormat.Underline == UnderlineType.Single) ? UnderlineType.None : UnderlineType.Single;
      }
    }
  }

  public bool IsSelectionStrikethrough
  {
    get;
    set
    {
      SetProperty(ref field, value);
      if (!_isUpdatingSelectionFormatStates)
      {
        Document.Selection.CharacterFormat.Strikethrough = FormatEffect.Toggle;
      }
    }
  }
  #endregion

  #region Font Size
  public readonly ImmutableList<string> FontSizes = ["8", "9", "10.5", "12", "14", "16", "18", "20", "24", "28", "32", "36", "48", "60", "72"];

  public string SelectionFontSizeText
  {
    get;
    set
    {
      if (ValidateFontSize(ref value))
      {
        SetProperty(ref field, value);
        if (!_isUpdatingSelectionFormatStates)
        {
          Document.Selection.CharacterFormat.Size = SelectionFontSize;
        }
      }
      else
      {
        SetProperty(ref field, string.Empty);
      }

      DecreaseSelectionFontSizeCommand?.NotifyCanExecuteChanged();
      IncreaseSelectionFontSizeCommand?.NotifyCanExecuteChanged();
    }
  } = string.Empty;

  public float SelectionFontSize => float.TryParse(SelectionFontSizeText, out float fontSize) ? fontSize : 10.5f;

  private static readonly float _minEditorFontSize = 5.0f;
  private static readonly float _maxEditorFontSize = 512.0f;

  private static bool ValidateFontSize(ref string fontSizeText)
  {
    if (!string.IsNullOrWhiteSpace(fontSizeText)
      && float.TryParse(fontSizeText, out float fontSize)
      && fontSize >= _minEditorFontSize
      && fontSize <= _maxEditorFontSize)
    {
      float eps = 1e-6f;
      float truncated = (float)Math.Truncate(fontSize * 100) / 100f;
      float fraction = Math.Abs(fontSize - (float)Math.Floor(fontSize));

      if (Math.Abs(fraction - 0.0f) < eps || Math.Abs(fraction - 0.5f) < eps)
      {
        if (Math.Abs(fontSize - truncated) < eps)
        {
          return true;
        }
      }
    }

    fontSizeText = string.Empty;
    return false;
  }
  #endregion

  #region Font Color
  public IReadOnlyList<SolidColorBrush> PaletteFontColors => AppColors.DefaultPaletteColorBrushes;

  public SolidColorBrush? SelectedPaletteFontColor
  {
    get;
    set
    {
      if (SetProperty(ref field, value) && value is not null)
      {
        SelectionFontColor = value.Color;
      }
    }
  }

  [ObservableProperty]
  public partial Color RecentFontColor { get; set; } = Colors.Black;

  public Color SelectionFontColor
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        SelectedPaletteFontColor = PaletteFontColors.FirstOrDefault(b => b.Color == value);
        if (!_isUpdatingSelectionFormatStates)
        {
          Document.Selection.CharacterFormat.ForegroundColor = value;
          RecentFontColor = value;
        }
      }
    }
  }
  #endregion

  #region Text Highlight Color
  public IReadOnlyList<SolidColorBrush> PaletteHighlightColors => AppColors.DefaultPaletteColorBrushes;

  public SolidColorBrush? SelectedPaletteHighlightColor
  {
    get;
    set
    {
      if (SetProperty(ref field, value) && value is not null)
      {
        SelectionHighlightColor = value.Color;
      }
    }
  }

  [ObservableProperty]
  public partial Color RecentHighlightColor { get; set; } = Colors.Transparent;

  public Color SelectionHighlightColor
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        SelectedPaletteHighlightColor = PaletteHighlightColors.FirstOrDefault(b => b.Color == value);
        ChangeSelectionHighlightColorToAutomaticCommand?.NotifyCanExecuteChanged();

        if (!_isUpdatingSelectionFormatStates)
        {
          Document.Selection.CharacterFormat.BackgroundColor = value;
          RecentHighlightColor = value;
        }
      }
    }
  }
  #endregion

  #region Bullets and Numbering
  public bool IsSelectionParagraphList
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        if (!_isUpdatingSelectionFormatStates)
        {
          Document.Selection.ParagraphFormat.ListType = value ? _recentMarkerType : MarkerType.None;
          Document.Selection.ParagraphFormat.ListStyle = value ? _recentMarkerStyle : MarkerStyle.Undefined;
        }
      }
    }
  }

  public static readonly BijectiveMap<MarkerType, string> _markerTypeMap = new()
  {
    { MarkerType.None,  "\u25cc" },
    { MarkerType.Bullet, "\u2022" },
    { MarkerType.Arabic, "1" },
    { MarkerType.LowercaseEnglishLetter, "a" },
    { MarkerType.UppercaseEnglishLetter,  "A" },
    { MarkerType.LowercaseRoman, "\u2170" },
    { MarkerType.UppercaseRoman, "\u2160" },
    { MarkerType.CircledNumber, "\u2460" }
  };
  public IReadOnlyBijectiveMap<MarkerType, string> MarkerTypeMap => _markerTypeMap;

  public static readonly BijectiveMap<MarkerStyle, string> _markerStyleMap = new()
  {
    { MarkerStyle.Parenthesis, ")" },
    { MarkerStyle.Parentheses, "( )" },
    { MarkerStyle.Period, "." },
    { MarkerStyle.Minus,  "-" },
  };
  public IReadOnlyBijectiveMap<MarkerStyle, string> MarkerStyleMap => _markerStyleMap;

  private MarkerType _recentMarkerType = MarkerType.Bullet;
  private MarkerStyle _recentMarkerStyle = MarkerStyle.Parenthesis;

  public MarkerType SelectionMarkerType
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        SelectedMarkerType = MarkerTypeMap.TryGetRight(value, out var markerType) ? markerType : null;
        if (!_isUpdatingSelectionFormatStates)
        {
          Document.Selection.ParagraphFormat.ListType = value;
          if (value is not MarkerType.None)
          {
            _recentMarkerType = value;
          }
        }
      }
    }
  }

  public string? SelectedMarkerType
  {
    get;
    set
    {
      if (SetProperty(ref field, value) && value is not null)
      {
        SelectionMarkerType = MarkerTypeMap.TryGetLeft(value, out var markerType) ? markerType : MarkerType.Undefined;
      }
    }
  }

  public MarkerStyle SelectionMarkerStyle
  {
    get;
    set
    {
      if (SetProperty(ref field, value))
      {
        SelectedMarkerStyle = SelectionMarkerType switch
        {
          MarkerType.None or MarkerType.Undefined or MarkerType.Bullet => null,
          _ => MarkerStyleMap.TryGetRight(value, out var markerStyle) ? markerStyle : null
        };

        if (!_isUpdatingSelectionFormatStates)
        {
          Document.Selection.ParagraphFormat.ListStyle = value;
          _recentMarkerStyle = value;
        }
      }
    }
  }

  public string? SelectedMarkerStyle
  {
    get;
    set
    {
      if (SetProperty(ref field, value) && value is not null)
      {
        SelectionMarkerStyle = MarkerStyleMap.TryGetLeft(value, out var markerStyle) ? markerStyle : MarkerStyle.Undefined;
      }
    }
  }

  [ObservableProperty]
  public partial bool IsSelectionMarkerStyleEnabled { get; set; }
  #endregion
}

partial class NoteEditorViewModel
{
  public Command UpdateSelectionCommand { get; private set; }
  public Command UpdateTextChangingCommand { get; private set; }
  public Command UpdateTextChangedCommand { get; private set; }
  public Command DecreaseSelectionFontSizeCommand { get; private set; }
  public Command IncreaseSelectionFontSizeCommand { get; private set; }
  public Command ChangeSelectionFontColorCommand { get; private set; }
  public Command ChangeSelectionHighlightColorCommand { get; private set; }
  public Command ChangeSelectionHighlightColorToAutomaticCommand { get; private set; }
  public Command EnterEditModeCommand { get; private set; }
  public AsyncCommand BrowseBackgroundImageCommand { get; private set; }

  private int _previousSelectionIndex = 0;
  private int _currentSelectionIndex = 0;

  private bool _shouldChangePreview = false;
  public readonly int PreviewTextMaxLength = 500;

  private readonly DispatcherTimer _editorThrottleTimer = new() { Interval = TimeSpan.FromMilliseconds(2000) };
  private bool _isEditorThrottling = false;

  private void EditorDebounceTimer_Tick(object? sender, object e)
  {
    Console.WriteLine("{0}: {1}", "Editor Tick", DateTimeOffset.Now);

    _editorThrottleTimer.Stop();
    _isEditorThrottling = false;
    ReflectEditorBodyChanges();
  }

  public void ReflectEditorBodyChanges()
  {
    Document.GetText(TextGetOptions.FormatRtf, out var editorText);
    editorText = AppRegexes.LastParInRtfRegex().Replace(editorText, "}");

    if (Note.Body != editorText)
    {
      Note.Body = editorText;
    }

    if (_shouldChangePreview)
    {
      Note.Preview = RtfTextConverter.GetPreview(Note.Body, 0, PreviewTextMaxLength);
    }

    _shouldChangePreview = false;
  }

  [MemberNotNull(nameof(UpdateSelectionCommand), nameof(UpdateTextChangingCommand), nameof(UpdateTextChangedCommand), nameof(DecreaseSelectionFontSizeCommand), nameof(IncreaseSelectionFontSizeCommand), nameof(ChangeSelectionFontColorCommand), nameof(ChangeSelectionHighlightColorCommand), nameof(ChangeSelectionHighlightColorToAutomaticCommand), nameof(EnterEditModeCommand), nameof(BrowseBackgroundImageCommand))]
  private void SetCommands()
  {
    UpdateSelectionCommand = new()
    {
      ExecuteAction = () =>
      {
        _previousSelectionIndex = Document.Selection.GetIndex(0);
        UpdateSelectionFormatStates();
      }
    };

    UpdateTextChangingCommand = new()
    {
      ExecuteAction = () =>
      {
        _currentSelectionIndex = Document.Selection.GetIndex(0);
        _shouldChangePreview = _previousSelectionIndex <= PreviewTextMaxLength && _currentSelectionIndex <= PreviewTextMaxLength;
      }
    };

    UpdateTextChangedCommand = new()
    {
      ExecuteAction = () =>
      {
        UpdateSelectionFormatStates();

        if (_isEditorThrottling)
        {
          return;
        }

        _isEditorThrottling = true;
        _editorThrottleTimer.Start();
      }
    };

    DecreaseSelectionFontSizeCommand = new()
    {
      ExecuteAction = () =>
      {
        var newFontSize = SelectionFontSize switch
        {
          5 => 5,
          > 5 and <= 12 => SelectionFontSize - 0.5f,
          <= 20 => SelectionFontSize - 1.0f,
          <= 32 => SelectionFontSize.LessThanNearestMultiple(2),
          <= 64 => SelectionFontSize.LessThanNearestMultiple(4),
          <= 512 => SelectionFontSize.LessThanNearestMultiple(8),
          _ => 0
        };
        Document.Selection.CharacterFormat.Size = newFontSize;
        SelectionFontSizeText = newFontSize.ToString();
      },
      CanExecuteFunc = () => SelectionFontSize > _minEditorFontSize
    };

    IncreaseSelectionFontSizeCommand = new()
    {
      ExecuteAction = () =>
      {
        var newFontSize = SelectionFontSize switch
        {
          >= 5 and < 12 => SelectionFontSize + 0.5f,
          < 20 => SelectionFontSize + 1.0f,
          < 32 => SelectionFontSize.GreaterThanNearestMultiple(2),
          < 64 => SelectionFontSize.GreaterThanNearestMultiple(4),
          < 512 => SelectionFontSize.GreaterThanNearestMultiple(8),
          512 => 512,
          _ => 0
        };
        Document.Selection.CharacterFormat.Size = newFontSize;
        SelectionFontSizeText = newFontSize.ToString();
      },
      CanExecuteFunc = () => SelectionFontSize < _maxEditorFontSize
    };

    ChangeSelectionFontColorCommand = new()
    {
      ExecuteAction = () => Document.Selection.CharacterFormat.ForegroundColor = RecentFontColor
    };

    ChangeSelectionHighlightColorCommand = new()
    {
      ExecuteAction = () => Document.Selection.CharacterFormat.BackgroundColor = RecentHighlightColor
    };

    ChangeSelectionHighlightColorToAutomaticCommand = new()
    {
      ExecuteAction = () => SelectionHighlightColor = Colors.Transparent,
      CanExecuteFunc = () => SelectionHighlightColor != Colors.White
    };

    EnterEditModeCommand = new()
    {
      ExecuteAction = () => Note.IsTextEditorReadOnly = false
    };

    BrowseBackgroundImageCommand = new()
    {
      ExecuteFunc = async () =>
      {
        if (NoteWindowService.TryGetWindowInfo(Note.Id, out _, out var appWindow))
        {
          PickFileResult pickFileResult = await new FileOpenPicker(appWindow.Id).PickSingleFileAsync();
          if (pickFileResult is not null)
          {
            Note.BackgroundImagePath = pickFileResult.Path;
          }
        }
      }
    };
  }
}