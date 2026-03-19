using System.Security.Cryptography;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Windows.Storage.Pickers;

using MyNotes.AppConstants;
using MyNotes.Common.Collections;
using MyNotes.Common.Commands;
using MyNotes.Helpers;
using MyNotes.Models.Notes;
using MyNotes.Services.App;

namespace MyNotes.ViewModels.Notes;

internal sealed partial class NoteEditorViewModel : ViewModelBase
{
  private readonly WindowService WindowService;
  private readonly Note Note;
  private readonly RichEditTextDocument Document;

  #region Object Lifetime Management
  public NoteEditorViewModel(WindowService windowService, Note note, RichEditTextDocument document)
  {
    WindowService = windowService;
    Note = note;
    Document = document;
    _editorDebounceTimer.Tick += EditorDebounceTimer_Tick;

    foreach (var imageFileName in Note.Images)
    {
      try
      {
        BitmapImage image = new() { UriSource = new Uri(System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, AppStrings.ImageFolderPath, imageFileName)) };
        Images.Add(image);
      }
      catch
      { }
    }
    IsImagePanelVisible = Images.Count > 0;

    Images.CollectionChanged += Images_CollectionChanged;
    SetCommands();
  }

  protected override void Dispose(bool disposing)
  {
    if (Disposed)
      return;

    if (disposing)
    {
      Images.CollectionChanged -= Images_CollectionChanged;
    }

    base.Dispose(disposing);
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

      DecreaseSelectionFontSizeCommand?.RaiseCanExecuteChanged();
      IncreaseSelectionFontSizeCommand?.RaiseCanExecuteChanged();
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
        ChangeSelectionHighlightColorToAutomaticCommand?.RaiseCanExecuteChanged();

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
            _recentMarkerType = value;
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

  #region Images
  public ObservableCollection<BitmapImage> Images = new();

  [ObservableProperty]
  public partial bool IsImagePanelVisible { get; set; }

  [ObservableProperty]
  public partial double ImagePanelMaxHeight { get; set; } = 120.0;

  private void Images_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
  {
    Note.Images = [.. Images.Select(image => System.IO.Path.GetFileName(image.UriSource.AbsolutePath))];
    IsImagePanelVisible = Images.Count > 0;
  }
  #endregion
}

internal sealed partial class NoteEditorViewModel : ViewModelBase
{
  public Command<object>? UpdateSelectionCommand { get; private set; }
  public Command<object>? UpdateTextChangingCommand { get; private set; }
  public Command<object>? UpdateTextChangedCommand { get; private set; }
  public Command? DecreaseSelectionFontSizeCommand { get; private set; }
  public Command? IncreaseSelectionFontSizeCommand { get; private set; }
  public Command<object>? ChangeSelectionFontColorCommand { get; private set; }
  public Command<object>? ChangeSelectionHighlightColorCommand { get; private set; }
  public Command? ChangeSelectionHighlightColorToAutomaticCommand { get; private set; }

  public Command? InsertImageCommand { get; private set; }

  private int _previousSelectionIndex = 0;
  private int _currentSelectionIndex = 0;
  private static readonly byte _editorDebounceCountThreshold = 20;
  private byte _editorDebounceCount = 0;
  private bool _shouldChangePreview = false;
  public readonly int PreviewTextMaxLength = 100;

  private readonly DispatcherTimer _editorDebounceTimer = new() { Interval = TimeSpan.FromMilliseconds(2000) };

  private void EditorDebounceTimer_Tick(object? sender, object e)
  {
    _editorDebounceTimer.Stop();
    UpdateEditorBodyText();
  }

  public void UpdateEditorBodyText()
  {
    Document.GetText(TextGetOptions.FormatRtf, out var editorText);
    editorText = AppRegexes.LastParInRtfRegex().Replace(editorText, "}");
    Document.GetText(TextGetOptions.None, out var plainText);
    Note.Body = editorText;
    Note.BodyPlainText = plainText;
    if (_shouldChangePreview)
      WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(true), AppMessageTokens.UpdateNotePreviewToken(Note.Id));
    _shouldChangePreview = false;
  }

  private void SetCommands()
  {
    UpdateSelectionCommand = new(
      actionToExecute: _ =>
      {
        _previousSelectionIndex = Document.Selection.GetIndex(0);
        UpdateSelectionFormatStates();
      });

    UpdateTextChangingCommand = new(
      actionToExecute: _ =>
      {
        _currentSelectionIndex = Document.Selection.GetIndex(0);
        _shouldChangePreview = _previousSelectionIndex <= PreviewTextMaxLength && _currentSelectionIndex <= PreviewTextMaxLength;
      });

    UpdateTextChangedCommand = new(
      actionToExecute: _ =>
      {
        UpdateSelectionFormatStates();

        _editorDebounceTimer.Stop();

        if (_editorDebounceCount++ >= _editorDebounceCountThreshold)
        {
          UpdateEditorBodyText();
          _editorDebounceCount = 0;
        }
        else
        {
          _editorDebounceTimer.Start();
        }
      });

    DecreaseSelectionFontSizeCommand = new(
      actionToExecute: () =>
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
      canExecuteFunc: () => SelectionFontSize > _minEditorFontSize
    );

    IncreaseSelectionFontSizeCommand = new(
      actionToExecute: () =>
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
      canExecuteFunc: () => SelectionFontSize < _maxEditorFontSize
    );

    ChangeSelectionFontColorCommand = new(
      actionToExecute: _ =>
      {
        Document.Selection.CharacterFormat.ForegroundColor = RecentFontColor;
      });

    ChangeSelectionHighlightColorCommand = new(
      actionToExecute: _ =>
      {
        Document.Selection.CharacterFormat.BackgroundColor = RecentHighlightColor;
      });

    ChangeSelectionHighlightColorToAutomaticCommand = new(
      actionToExecute: () =>
      {
        SelectionHighlightColor = Colors.Transparent;
      },
      canExecuteFunc: () => SelectionHighlightColor != Colors.White
    );

    InsertImageCommand = new()
    {
      ActionToExecute = async () =>
      {
        if (WindowService.TryGetNoteWindowInfo(Note.Id, out _, out var appWindow))
        {
          FileOpenPicker picker = new(appWindow.OwnerWindowId)
          {
            ViewMode = PickerViewMode.Thumbnail
          };
          picker.FileTypeFilter.Add(".jpg");
          picker.FileTypeFilter.Add(".jpeg");
          picker.FileTypeFilter.Add(".png");
          picker.FileTypeFilter.Add(".bmp");
          picker.FileTypeFilter.Add(".gif");
          picker.FileTypeFilter.Add(".tiff");
          picker.FileTypeFilter.Add(".ico");

          foreach (var result in await picker.PickMultipleFilesAsync())
          {
            try
            {
              var originalFile = await StorageFile.GetFileFromPathAsync(result.Path);

              string fileName;
              using (var stream = await originalFile.OpenStreamForReadAsync())
              using (var md5 = MD5.Create())
              {
                fileName = System.IO.Path.ChangeExtension(Convert.ToHexStringLower(await md5.ComputeHashAsync(stream)), System.IO.Path.GetExtension(result.Path));
              }

              var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(AppStrings.ImageFolderPath, CreationCollisionOption.OpenIfExists);
              var copiedFile = await originalFile.CopyAsync(folder, fileName, NameCollisionOption.ReplaceExisting);
              BitmapImage image = new() { UriSource = new Uri(copiedFile.Path) };
              Images.Add(image);
            }
            catch (Exception e)
            {
              Console.WriteLine("{0}: {1}", "File Exception", e.Message);
            }
          }
        }
      }
    };
  }
}