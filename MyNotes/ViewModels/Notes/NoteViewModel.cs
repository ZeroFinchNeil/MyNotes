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
  private static readonly ImmutableDictionary<string, Func<Note, Action<NoteEntity>>> _notePropertyToEntityActions = ImmutableDictionary.CreateRange(new Dictionary<string, Func<Note, Action<NoteEntity>>>()
    {
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
    Note.PropertyChanged += Note_PropertyChanged;
  }

  private readonly HashSet<string> _changedNoteProperties = new();

  private async void NotePropertyChangedDebounceTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e) => await UpdateNoteEntity();

  private async Task UpdateNoteEntity()
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

  private void Note_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    _notePropertyDebounceTimer.Start();
    // 뷰에 반영
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

  private void SetCommand()
  {
    SaveCommand = new(
      actionToExecute: () =>
      {
        Console.WriteLine("{0}: {1}", "Save Note.", "");
      });
  }
}
