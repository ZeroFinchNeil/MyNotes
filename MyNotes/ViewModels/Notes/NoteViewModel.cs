using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media.Imaging;

using MyNotes.AppConstants;
using MyNotes.Common.Commands;
using MyNotes.Common.Messages;
using MyNotes.Common.Structures;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.App;
using MyNotes.Services.Commands;
using MyNotes.Services.Database.Entities;
using MyNotes.Services.Notes;
using MyNotes.Templates.Media;

using Windows.Storage.Streams;

namespace MyNotes.ViewModels.Notes;

internal sealed partial class NoteViewModel : ViewModelBase
{
  private static readonly ImmutableDictionary<string, Func<Note, Action<NoteEntity>>> _notePropertyToDbContextEntityActions = ImmutableDictionary.CreateRange(new Dictionary<string, Func<Note, Action<NoteEntity>>>()
  {
    { nameof(Note.NavigationId), note => e => e.Parent = note.NavigationId.Value },
    { nameof(Note.Modified), note => e => e.Modified = note.Modified },
    { nameof(Note.Title), note => e => e.Title = note.Title },
    { nameof(Note.Body), note => e => e.Body = note.Body },
    { nameof(Note.BackgroundColor), note => e => e.BackgroundColor = note.BackgroundColor.ToString() },
    { nameof(Note.IsBackgroundImageVisible), note => e => e.IsBackgroundImageVisible = note.IsBackgroundImageVisible },
    { nameof(Note.BackgroundImagePath), note => e => e.BackgroundImagePath = note.BackgroundImagePath },
    { nameof(Note.BackgroundImageOpacity), note => e => e.BackgroundImageOpacity = note.BackgroundImageOpacity },
    { nameof(Note.BackgroundImageBlur), note => e => e.BackgroundImageBlur = note.BackgroundImageBlur },
    { nameof(Note.BackdropKind), note => e => e.BackdropKind = (int)note.BackdropKind },
    { nameof(Note.BackdropTintOpacity), note => e => e.BackdropTintOpacity = Math.Round( note.BackdropTintOpacity, 2) },
    { nameof(Note.BackdropLuminosityOpacity), note => e => e.BackdropLuminosityOpacity =  Math.Round(note.BackdropLuminosityOpacity, 2) },
    { nameof(Note.Size), note => e =>
      {
        e.Width = note.Size.Width;
        e.Height = note.Size.Height;
      }
    },
    { nameof(Note.Position), note => e =>
      {
        e.PositionX = note.Position.X;
        e.PositionY = note.Position.Y;
      }
    },
    { nameof(Note.IsBookmarked), note => e => e.IsBookmarked = note.IsBookmarked },
    { nameof(Note.IsDeleted), note => e => e.IsDeleted = note.IsDeleted },
    { nameof(Note.IsWindowOpen), note => e => e.IsWindowOpen = note.IsWindowOpen },
    { nameof(Note.IsAlwaysOnTop), note => e => e.IsAlwaysOnTop = note.IsAlwaysOnTop }
  });

  private static readonly ImmutableHashSet<string> _notePropertyToNoteSearchEntity = [nameof(Note.Title), nameof(Note.BodyPlainText)];

  private readonly WindowService WindowService;
  private readonly NoteCommandService NoteCommandService;
  private readonly NoteService NoteService;
  private readonly JumpListService JumpListService;

  public Note Note { get; }

  #region Object Lifetime Management
  public NoteViewModel(WindowService windowService, [FromKeyedServices(CommandServiceType.Note)] ICommandService noteCommandService, NoteService noteService, JumpListService jumpListService, Note note)
  {
    // DI
    WindowService = windowService;
    NoteCommandService = (NoteCommandService)noteCommandService;
    NoteService = noteService;
    JumpListService = jumpListService;

    Note = note;

    _notePropertyDebounceTimer.Elapsed += NotePropertyChangedDebounceTimer_Elapsed;

    _selectedPaletteBackgroundColor = PaletteBackgroundColors.FirstOrDefault(b => b.Color == Note.BackgroundColor);
    BackgroundImage = Note.IsBackgroundImageVisible ? GetBackgroundImage(Note.BackgroundImagePath) : null;
    Preview = GetPreview(Note.Body, 0, PreviewTextMaxLength);
    Note.PropertyChanged += Note_PropertyChanged;
    RegisterMessengers();
  }

  protected override void Dispose(bool disposing)
  {
    if (Disposed)
      return;

    if (disposing)
    {
      _notePropertyDebounceTimer.Dispose();
      Note.PropertyChanged -= Note_PropertyChanged;
      _ = UpdateNoteEntity();
      _ = NoteService.CommitSearchIndexAsync();
      UnregisterMessengers();
    }

    base.Dispose(disposing);
  }
  #endregion

  private async void Note_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    _notePropertyDebounceTimer.Start();

    // 뷰에 반영(TwoWay 바인딩 시) 
    switch (e.PropertyName)
    {
      case nameof(Note.Title):
        await JumpListService.EditJumpListItemAsync(Note);
        break;
      case nameof(Note.BackdropKind):
        ChangeNoteBackdrop();
        if (Note.BackdropKind is not BackdropKind.None)
        {
          Note.IsBackgroundImageVisible = false;
        }
        break;
      case nameof(Note.BackdropKind) or nameof(Note.BackdropTintOpacity) or nameof(Note.BackdropLuminosityOpacity):
        ChangeNoteBackdropProperties();
        break;
      case nameof(Note.BackgroundColor):
        SelectedPaletteBackgroundColor = PaletteBackgroundColors.FirstOrDefault(b => b.Color == Note.BackgroundColor);
        ChangeNoteBackdropProperties();
        break;
      case nameof(Note.IsBookmarked):
        WeakReferenceMessenger.Default.Send(new PropertyChangedMessage<bool>(Note, nameof(Note.IsBookmarked), !Note.IsBookmarked, Note.IsBookmarked), AppMessageTokens.ChangeNoteIsBookmarkedStateToken);
        break;
      case nameof(Note.IsDeleted):
        break;
      case nameof(Note.IsBackgroundImageVisible) or nameof(Note.BackgroundImagePath):
        BackgroundImage = Note.IsBackgroundImageVisible ? GetBackgroundImage(Note.BackgroundImagePath) : null;
        if (Note.IsBackgroundImageVisible)
        {
          Note.BackdropKind = BackdropKind.None;
        }
        break;
      case nameof(Note.IsAlwaysOnTop):
        WindowService.TryExecuteOnNoteWindow(Note.Id, noteWindow =>
        {
          (noteWindow.AppWindow.Presenter as OverlappedPresenter)?.IsAlwaysOnTop = Note.IsAlwaysOnTop;
        });
        break;
    }

    if (!string.IsNullOrEmpty(e.PropertyName))
    {
      _changedNoteProperties.Add(e.PropertyName);
    }
  }

  private readonly HashSet<string> _changedNoteProperties = new();

  private async void NotePropertyChangedDebounceTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e) => await UpdateNoteEntity();

  public async Task UpdateNoteEntity()
  {
    Action<NoteEntity>? dbActions = null;
    bool _updateNoteIndex = false;
    foreach (var propertyName in _changedNoteProperties)
    {
      if (_notePropertyToDbContextEntityActions.TryGetValue(propertyName, out var dbAction))
      {
        dbActions += dbAction(Note);
      }
      if (_notePropertyToNoteSearchEntity.Contains(propertyName))
        _updateNoteIndex = true;
    }

    if (dbActions is not null)
    {
      await NoteService.UpdateNoteEntityAsync(Note, dbActions);
    }

    if (_updateNoteIndex)
    {
      await NoteService.UpdateNoteSearchEntityAsync(Note);
    }

    _changedNoteProperties.Clear();
  }

  public Task<bool> DeleteNoteEntity() => NoteService.DeleteNotePermanentlyAsync(Note.Id);

  private static readonly double _notePropertyDebounceTimerInterval = 500;
  private readonly System.Timers.Timer _notePropertyDebounceTimer = new() { Interval = _notePropertyDebounceTimerInterval, AutoReset = false };

  private readonly RichEditBox _previewRichEditBox = new();
  private string GetPreview(string body, int start, int end)
  {
    var document = _previewRichEditBox.Document;
    document.SetText(TextSetOptions.FormatRtf, body);
    document.Selection.SetRange(start, end);
    document.Selection.GetText(TextGetOptions.FormatRtf, out var preview);
    return preview;
  }

  public void SetPreview()
  {
    Preview = GetPreview(Note.Body, 0, PreviewTextMaxLength);
  }

  private ImageSource? GetBackgroundImage(string? imagePath)
  {
    if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
    {
      return null;
    }

    try
    {
      BitmapImage image = new() { UriSource = new Uri(imagePath) };
      return image;
    }
    catch (Exception)
    { }

    return null;
  }

  public string Preview
  {
    get => field;
    set => SetProperty(ref field, value);
  }

  public int PreviewTextMaxLength
  {
    get;
    set => SetProperty(ref field, value);
  } = 100;

  #region Backdrop and Background Color

  public IReadOnlyList<BackdropKind> BackdropKinds { get; } = Enum.GetValues<BackdropKind>();

  public void ChangeNoteBackdrop()
  {
    WindowService.TryExecuteOnNoteWindow(Note.Id, (noteWindow) =>
    {
      noteWindow.SystemBackdrop = Note.BackdropKind switch
      {
        BackdropKind.Acrylic => new ExtendedAcrylicBackdrop()
        {
          TintColor = Note.BackgroundColor,
          TintOpacity = Note.BackdropTintOpacity,
          LuminosityOpacity = Note.BackdropLuminosityOpacity,
          FallbackColor = GetFallbackColor(Note.BackgroundColor, Note.BackdropTintOpacity)
        },
        BackdropKind.Mica => new ExtendedMicaBackdrop()
        {
          TintColor = Note.BackgroundColor,
          TintOpacity = Note.BackdropTintOpacity,
          LuminosityOpacity = Note.BackdropLuminosityOpacity,
          FallbackColor = GetFallbackColor(Note.BackgroundColor, Note.BackdropTintOpacity)
        },
        BackdropKind.None or _ => null
      };
    });
  }

  private void ChangeNoteBackdropProperties()
  {
    WindowService.TryExecuteOnNoteWindow(Note.Id, (noteWindow) =>
    {
      if (noteWindow.SystemBackdrop is ExtendedSystemBackdrop backdrop)
      {
        backdrop.TintColor = Note.BackgroundColor;
        backdrop.TintOpacity = Note.BackdropTintOpacity;
        backdrop.LuminosityOpacity = Note.BackdropLuminosityOpacity;
        backdrop.FallbackColor = GetFallbackColor(Note.BackgroundColor, Note.BackdropTintOpacity);
      }
    });
  }

  private Color GetFallbackColor(Color color, double opacity) => Color.FromArgb((byte)(opacity * 255), color.R, color.G, color.B);

  public ImageSource? BackgroundImage
  {
    get => field;
    set => SetProperty(ref field, value);
  }

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
  #endregion
}

internal sealed partial class NoteViewModel : ViewModelBase
{
  public Command<Note> OpenWindowCommand => NoteCommandService.OpenNoteWindowCommand;
  public Command<SourceTargetPair<Note, NavigationId>> MoveToListCommand => NoteCommandService.MoveNoteToListCommand;
  public Command<NavigationId?> CreateNewNoteCommand => NoteCommandService.CreateNewNoteCommand;
  public Command<Note> ViewListCommand => NoteCommandService.ViewListCommand;

  public Command<Note> RemoveNoteCommand => NoteCommandService.RemoveNoteCommand;

  public Command<Note> AddNoteToJumpListCommand => NoteCommandService.AddNoteToJumpListCommand;

  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<ValueChangedMessage<bool>, MessageToken<NoteId>>(this, AppMessageTokens.UpdateNotePreviewToken(Note.Id), (recipient, message) =>
    {
      SetPreview();
    });
  }

  private void UnregisterMessengers()
  {
    WeakReferenceMessenger.Default.UnregisterAll(this);
  }
}