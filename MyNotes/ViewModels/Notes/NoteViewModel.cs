using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Commands;
using MyNotes.Debugging;
using MyNotes.Models.Notes;
using MyNotes.Services.Commands;
using MyNotes.Services.Database.Entities;
using MyNotes.Services.Notes;

namespace MyNotes.ViewModels.Notes;

internal sealed partial class NoteViewModel : ViewModelBase
{
  private readonly NoteViewModelCommandService NoteViewModelCommandService;
  private readonly NoteService NoteService;
  public Note Note { get; }

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

    NotePropertyDbActions = ImmutableDictionary.CreateRange(new Dictionary<string, Action<NoteEntity>>()
    {
      { nameof(Note.Modified), e => e.Modified = Note.Modified },
      { nameof(Note.Title), e => e.Title = Note.Title },
      { nameof(Note.Body), e => e.Body = Note.Body },
      { nameof(Note.Background), e => e.Background = Note.Background.ToString() },
      { nameof(Note.Backdrop), e => e.Backdrop = (int)Note.Backdrop },
      { nameof(Note.Size), e =>
        {
          e.Width = Note.Size.Width;
          e.Height = Note.Size.Height;
        }
      },
      { nameof(Note.Position), e =>
        {
          e.PositionX = Note.Position.X;
          e.PositionY = Note.Position.Y;
        }
      },
      { nameof(Note.IsBookmarked), e => e.IsBookmarked = Note.IsBookmarked },
      { nameof(Note.IsDeleted), e => e.IsDeleted = Note.IsDeleted },
    });

    _notePropertyChangedDebounceTimer.Elapsed += NotePropertyChangedDebounceTimer_Elapsed;
    Note.PropertyChanged += Note_PropertyChanged;
  }

  private readonly ImmutableDictionary<string, Action<NoteEntity>> NotePropertyDbActions;

  private readonly HashSet<string> ChangedNoteProperties = new();

  private async void NotePropertyChangedDebounceTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e) => await UpdateNoteEntity();

  private async Task UpdateNoteEntity()
  {
    Action<NoteEntity>? actions = null;
    foreach (var propertyName in ChangedNoteProperties)
    {
      if (NotePropertyDbActions.TryGetValue(propertyName, out var action))
      {
        actions += action;
      }
    }

    if (actions is not null)
      await NoteService.UpdateNoteEntityAsync(Note, actions);
  }

  private static readonly double _notePropertyChangedDebounceTimerInterval = 2000;
  private readonly System.Timers.Timer _notePropertyChangedDebounceTimer = new() { Interval = _notePropertyChangedDebounceTimerInterval, AutoReset = false };

  private void Note_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    _notePropertyChangedDebounceTimer.Start();
    // 뷰에 반영
    switch (e.PropertyName)
    {
      case nameof(Note.Backdrop):
        this.Backdrop = (int)Note.Backdrop;
        break;
    }

    if (!string.IsNullOrEmpty(e.PropertyName))
    {
      ChangedNoteProperties.Add(e.PropertyName);
    }
  }

  public int Backdrop
  {
    get;
    set
    {
      if (field != value)
      {
        SetProperty(ref field, value);
        Note.Backdrop = (BackdropKind)value;
      }
    }
  }

  protected override void Dispose(bool disposing)
  {
    if (_disposed)
      return;

    if (disposing)
    {
      _notePropertyChangedDebounceTimer.Dispose();
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

  private void SetCommand()
  {
    SaveCommand = new(
      actionToExecute: () =>
      {
        Console.WriteLine("{0}: {1}", "Save Note.", "");
      });
  }
}
