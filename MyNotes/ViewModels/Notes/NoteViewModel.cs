using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Commands;
using MyNotes.Common.Structures;
using MyNotes.Constants;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.Commands;
using MyNotes.Services.Database.Entities;
using MyNotes.Services.Notes;

namespace MyNotes.ViewModels.Notes;

internal sealed partial class NoteViewModel : ViewModelBase
{
  private static readonly ImmutableDictionary<string, Func<Note, Action<NoteEntity>>> _notePropertyToDbContextEntityActions = ImmutableDictionary.CreateRange(new Dictionary<string, Func<Note, Action<NoteEntity>>>()
    {
      { nameof(Note.NavigationId), note => e => e.Parent = note.NavigationId.Value },
      { nameof(Note.Modified), note => e => e.Modified = note.Modified },
      { nameof(Note.Title), note => e => e.Title = note.Title },
      { nameof(Note.Body), note => e => e.Body = note.Body },
      { nameof(Note.Background), note => e => e.Background = note.Background.ToString() },
      { nameof(Note.Backdrop), note => e => e.Backdrop = (int)note.Backdrop },
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
      { nameof(Note.IsWindowOpen), note => e => e.IsWindowOpen = note.IsWindowOpen }
    });

  private static readonly ImmutableHashSet<string> _notePropertyToNoteSearchEntity = [nameof(Note.Title), nameof(Note.BodyPlainText)];

  private readonly NoteViewModelCommandService NoteViewModelCommandService;
  private readonly NoteService NoteService;

  public Note Note { get; }

  // 생성자
  public NoteViewModel([FromKeyedServices(CommandServiceType.NoteViewModel)] ICommandService commandService, NoteService noteService, Note note)
  {
    // DI
    NoteViewModelCommandService = (NoteViewModelCommandService)commandService;
    NoteService = noteService;

    Note = note;

    _notePropertyDebounceTimer.Elapsed += NotePropertyChangedDebounceTimer_Elapsed;

    _selectedPaletteBackgroundColor = PaletteBackgroundColors.FirstOrDefault(b => b.Color == Note.Background);
    _backdrop = (int)Note.Backdrop;
    _preview = GetPreview(Note.Body, 0, PreviewTextMaxLength);

    Note.PropertyChanged += Note_PropertyChanged;
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

  private static readonly double _notePropertyDebounceTimerInterval = 500;
  private readonly System.Timers.Timer _notePropertyDebounceTimer = new() { Interval = _notePropertyDebounceTimerInterval, AutoReset = false };

  private readonly RichEditBox _previewRichEditBox = new();
  public string GetPreview(string body, int start, int end)
  {
    var document = _previewRichEditBox.Document;
    document.SetText(TextSetOptions.FormatRtf, body);
    document.Selection.SetRange(start, end);
    document.Selection.GetText(TextGetOptions.FormatRtf, out var preview);
    return preview;
  }

  private void Note_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    _notePropertyDebounceTimer.Start();

    // 뷰에 반영(TwoWay 바인딩 시) 
    switch (e.PropertyName)
    {
      case nameof(Note.Backdrop):
        this.Backdrop = (int)Note.Backdrop;
        break;
      case nameof(Note.Background):
        SelectedPaletteBackgroundColor = PaletteBackgroundColors.FirstOrDefault(b => b.Color == Note.Background);
        break;
      case nameof(Note.IsBookmarked):
        WeakReferenceMessenger.Default.Send(new PropertyChangedMessage<bool>(Note, nameof(Note.IsBookmarked), !Note.IsBookmarked, Note.IsBookmarked), MessageTokens.ChangeNoteIsBookmarkedStateToken);
        break;
    }

    if (!string.IsNullOrEmpty(e.PropertyName))
    {
      _changedNoteProperties.Add(e.PropertyName);
    }
  }

  private string _preview;
  public string Preview
  {
    get => _preview;
    set => SetProperty(ref _preview, value);
  }

  public int PreviewTextMaxLength
  {
    get;
    set => SetProperty(ref field, value);
  } = 100;

  #region Backdrop and Background Color
  private int _backdrop;
  public int Backdrop
  {
    get => _backdrop;
    set
    {
      if (_backdrop != value)
      {
        SetProperty(ref _backdrop, value);
        Note.Backdrop = (BackdropKind)value;
      }
    }
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
        Note.Background = value.Color;
      }
    }
  }
  #endregion

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
    }

    base.Dispose(disposing);
  }
}

internal sealed partial class NoteViewModel : ViewModelBase
{
  public Command<NoteViewModel> OpenWindowCommand => NoteViewModelCommandService.OpenWindowCommand;
  public Command<SourceTargetPair<NoteViewModel, NavigationId>> MoveToListCommand => NoteViewModelCommandService.MoveToListCommand;
  public Command<NoteViewModel> CreateNewNoteCommand => NoteViewModelCommandService.CreateNewNoteCommand;
  public Command<NoteViewModel> ViewListCommand => NoteViewModelCommandService.ViewListCommand;
}