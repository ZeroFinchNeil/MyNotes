using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Commands;
using MyNotes.Common.Structures;
using MyNotes.Debugging;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.Commands;
using MyNotes.Services.Database.Entities;
using MyNotes.Services.Notes;
using MyNotes.ViewModels.Navigations;

namespace MyNotes.ViewModels.Notes;

internal sealed partial class NoteViewModel : ViewModelBase
{
  private static readonly ImmutableDictionary<string, Func<Note, Action<NoteEntity>>> _notePropertyToEntityActions = ImmutableDictionary.CreateRange(new Dictionary<string, Func<Note, Action<NoteEntity>>>()
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
      { nameof(Note.IsBookmarked), note =>e => e.IsBookmarked = note.IsBookmarked },
      { nameof(Note.IsDeleted), note => e => e.IsDeleted = note.IsDeleted },
    });

  private readonly NoteViewModelCommandService NoteViewModelCommandService;
  private readonly NoteService NoteService;

  public Note Note { get; }

  // 생성자
  public NoteViewModel([FromKeyedServices(CommandServiceType.NoteViewModel)] ICommandService commandService, NoteService noteService, Note note)
  {
#if DEBUG
    ReferenceTracker.NoteViewModelReference.Add(this, note.Id.Value);
#endif

    // DI
    NoteViewModelCommandService = (NoteViewModelCommandService)commandService;
    NoteService = noteService;

    Note = note;
    SetCommand();

    _notePropertyDebounceTimer.Elapsed += NotePropertyChangedDebounceTimer_Elapsed;

    _backdrop = (int)Note.Backdrop;
    _preview = GetPreview(Note.Body, 0, PreviewTextMaxLength);

    Note.PropertyChanged += Note_PropertyChanged;
  }

  private readonly HashSet<string> _changedNoteProperties = new();

  private async void NotePropertyChangedDebounceTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e) => await UpdateNoteEntity();

  public async Task UpdateNoteEntity()
  {
    Action<NoteEntity>? actions = null;
    foreach (var propertyName in _changedNoteProperties)
    {
      if (_notePropertyToEntityActions.TryGetValue(propertyName, out var action))
      {
        actions += action(Note);
      }
    }

    if (actions is not null)
    {
      await NoteService.UpdateNoteEntityAsync(Note, actions);
      _changedNoteProperties.Clear();
    }
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

    // 뷰에 반영(타입이 다른 TwoWay 바인딩 시) 
    switch (e.PropertyName)
    {
      case nameof(Note.Backdrop):
        this.Backdrop = (int)Note.Backdrop;
        break;
    }

    if (!string.IsNullOrEmpty(e.PropertyName))
    {
      _changedNoteProperties.Add(e.PropertyName);
    }
  }

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

  protected override void Dispose(bool disposing)
  {
    if (_disposed)
      return;

    if (disposing)
    {
      _notePropertyDebounceTimer.Dispose();
      Note.PropertyChanged -= Note_PropertyChanged;
      _ = UpdateNoteEntity();
    }

    _disposed = true;
  }
}

internal sealed partial class NoteViewModel : ViewModelBase
{
  public Command? SaveCommand { get; private set; }
  public Command<NoteViewModel> OpenWindowCommand => NoteViewModelCommandService.OpenWindowCommand;
  public Command<SourceTargetPair<NavigationId, NavigationId>> MoveToListCommand { get; private set; }

  private void SetCommand()
  {
    SaveCommand = new(
      actionToExecute: () =>
      {
        Console.WriteLine("{0}: {1}", "Save Note.", "");
      });

    MoveToListCommand = new(
      actionToExecute: async (pair) =>
      {
        Console.WriteLine("{0}: {1} {2}", "MoveToList", pair.Source.Value, pair.Target.Value);

        if (pair.Source == pair.Target)
          return;

        Note.NavigationId = pair.Target;
        await UpdateNoteEntity();

        var NavigationViewModelProvider = App.Instance.Services.GetRequiredService<NavigationViewModelProvider>();
        if (NavigationViewModelProvider.TryResolve(pair.Source, out var s)
            && s is UserLeafNavigationViewModel sourceViewModel)
        {
          sourceViewModel.NoteViewModels?.Remove(this);
        }
      });
  }
}